// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using Assimp;
using Assimp.Configs;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline
{
    /// <summary>
    /// Provides methods for reading AutoDesk (.fbx) files for use in the Content Pipeline.
    /// </summary>
    /// <remarks>
    /// Since OpenAssetImporter supports lots of formats, there's little that stands in the
    /// way of adding more file extensions to the importer attribute and suporting more.
    /// </remarks>
    [ContentImporter(".fbx", DisplayName = "OpenAssetImporter - MonoGame", DefaultProcessor = "ModelProcessor")]
    public class OpenAssetImporter : ContentImporter<NodeContent>
    {
        private ContentIdentity _identity;
        private Scene _scene;
        private NodeContent _rootNode;
        private List<MaterialContent> _materials;

        private Dictionary<string, Matrix> _bindPose = new Dictionary<string, Matrix>();

        public override NodeContent Import(string filename, ContentImporterContext context)
        {
            _identity = new ContentIdentity(filename, GetType().Name);
            var importer = new AssimpImporter();
            importer.AttachLogStream(new LogStream((msg, userData) => context.Logger.LogMessage(msg)));
            _scene = importer.ImportFile(filename,
                                            PostProcessSteps.FindInstances |
                                            PostProcessSteps.FindInvalidData |
                                            PostProcessSteps.FlipUVs |
                                            PostProcessSteps.FlipWindingOrder |
                                            PostProcessSteps.JoinIdenticalVertices |
                                            PostProcessSteps.ImproveCacheLocality |
                                            PostProcessSteps.OptimizeGraph |
                                            PostProcessSteps.OptimizeMeshes |
                                            PostProcessSteps.RemoveRedundantMaterials |
                                            PostProcessSteps.Triangulate
                );

            _rootNode = new NodeContent
            {
                Name = _scene.RootNode.Name,
                Identity = _identity,
                Transform = ToXna(_scene.RootNode.Transform)
            };

            _materials = ImportMaterials(_identity, _scene);

            FindMeshes(_scene.RootNode, _rootNode.Transform);

            var skeleton = CreateSkeleton();

            CreateAnimation(skeleton);

            return _rootNode;
        }

        private static List<MaterialContent> ImportMaterials(ContentIdentity identity, Scene scene)
        {
            var materials = new List<MaterialContent>();

            foreach (var sceneMaterial in scene.Materials)
            {
                var diffuse = sceneMaterial.TextureDiffuse;

                materials.Add(new BasicMaterialContent()
                {
                    Name = sceneMaterial.Name,
                    Identity = identity,
                    Texture = new ExternalReference<TextureContent>(diffuse.FilePath, identity)
                });
            }

            return materials;
        }

        private MeshContent CreateMesh(Mesh sceneMesh)
        {
            var mesh = new MeshContent { Name = sceneMesh.Name };

            // Position vertices are shared at the mesh level
            foreach (var vert in sceneMesh.Vertices)
                mesh.Positions.Add(new Vector3(vert.X, vert.Y, vert.Z));

            var geom = new GeometryContent
            {
                Name = string.Empty,
                Material = _materials[sceneMesh.MaterialIndex]
            };

            // Geometry vertices reference 1:1 with the MeshContent parent,
            // no indirection is necessary.
            //geom.Vertices.Positions.AddRange(mesh.Positions);
            geom.Vertices.AddRange(Enumerable.Range(0, sceneMesh.VertexCount));
            geom.Indices.AddRange(sceneMesh.GetIndices());

            // Individual channels go here
            if (sceneMesh.HasNormals)
                geom.Vertices.Channels.Add(VertexChannelNames.Normal(), ToXna(sceneMesh.Normals));

            for (var i = 0; i < sceneMesh.TextureCoordinateChannelCount; i++)
                geom.Vertices.Channels.Add(VertexChannelNames.TextureCoordinate(i),
                                           ToXnaTexCoord(sceneMesh.TextureCoordinateChannels[i]));

            mesh.Geometry.Add(geom);

            return mesh;
        }

        private void FindMeshes(Node aiNode, Matrix parentXform)
        {
            var transform = parentXform * ToXna(aiNode.Transform);

            foreach (var meshIndex in aiNode.MeshIndices)
            {
                var aiMesh = _scene.Meshes[meshIndex];

                // Extract bind pose.
                foreach (var bone in aiMesh.Bones)
                {
                    var boneName = bone.Name;
                    var bindPose = Matrix.Invert(ToXna(bone.OffsetMatrix));
                    _bindPose[boneName] = bindPose;
                }

                // Extract geometry
                var mesh = CreateMesh(aiMesh);
                mesh.Transform = transform;

                _rootNode.Children.Add(mesh);
            }

            // Children
            foreach (var child in aiNode.Children)
                FindMeshes(child, transform);
        }

        private NodeContent CreateSkeleton()
        {
            var bones = _scene.Meshes.SelectMany(m => m.Bones).Distinct().ToList();
            Bone rootBone = null;
            Node rootNode = null;

            FindSkeletonRoot(bones, _scene.RootNode, out rootNode, out rootBone);
            if (rootNode == null || rootBone == null)
                return null;

            return WalkHierarchy(rootNode, _rootNode, _rootNode.Transform);
        }

        private void CreateAnimation(NodeContent skeleton)
        {
            if (skeleton != null)
            {
                foreach (var animation in _scene.Animations)
                    skeleton.Animations.Add(animation.Name, CreateAnimation(animation));
            }
        }

        private static void FindSkeletonRoot(List<Bone> bones, Node sceneRoot, out Node rootNode, out Bone rootBone)
        {
            rootNode = null;
            rootBone = null;
            var minDepth = int.MaxValue;

            foreach (var bone in bones)
            {
                var node = sceneRoot.FindNode(bone.Name);

                // Walk up the tree to find the depth of this node
                var depth = 0;
                var walk = node;
                while (walk != sceneRoot)
                {
                    walk = walk.Parent;
                    depth++;
                }

                if (depth < minDepth)
                {
                    rootNode = node;
                    rootBone = bone;
                    minDepth = depth;
                }
            }
        }

        private NodeContent WalkHierarchy(Node aiNode, NodeContent parent, Matrix parentXform)
        {
            var transform = parentXform * ToXna(aiNode.Transform);
            var parentInHierarchy = parent;

            // BoneContent
            var node = aiNode.HasChildren
                ? new BoneContent()
                : new NodeContent();

            node.Name = aiNode.Name.Replace("_$Assimp_FbxNull$", string.Empty);
            node.Transform = transform;

            // Replace transform with the bind pose if we have one.
            //if (_bindPose.ContainsKey(aiNode.Name))
            //    node.Transform *= _bindPose[aiNode.Name];

            parent.Children.Add(node);

            parentInHierarchy = node;
            transform = Matrix.Identity;

            // Children
            foreach (var child in aiNode.Children)
                WalkHierarchy(child, parentInHierarchy, transform);

            return parentInHierarchy;
        }

        private AnimationContent CreateAnimation(Assimp.Animation aiAnimation)
        {
            var animation = new AnimationContent
            {
                Name = aiAnimation.Name,
                Duration = TimeSpan.FromSeconds(aiAnimation.DurationInTicks / aiAnimation.TicksPerSecond),
                Identity = _identity
            };

            foreach (var aiChannel in aiAnimation.NodeAnimationChannels)
            {
                var channel = new AnimationChannel();

                // We can have different numbers of keyframes for each, so find the max index.
                var keyCount = Math.Max(aiChannel.PositionKeyCount, Math.Max(aiChannel.RotationKeyCount, aiChannel.ScalingKeyCount));

                // Get all unique keyframe times
                var times = aiChannel.PositionKeys.Select(k => k.Time)
                    .Union(aiChannel.RotationKeys.Select(k => k.Time))
                    .Union(aiChannel.ScalingKeys.Select(k => k.Time))
                    .Distinct().ToList();

                foreach (var aiKeyTime in times)
                {
                    var time = TimeSpan.FromSeconds(aiKeyTime / aiAnimation.TicksPerSecond);
                    var position = aiChannel.PositionKeys.FirstOrDefault(k => k.Time == aiKeyTime);
                    var rotation = aiChannel.RotationKeys.FirstOrDefault(k => k.Time == aiKeyTime);
                    var scale = aiChannel.ScalingKeys.FirstOrDefault(k => k.Time == aiKeyTime);

                    var xform = Matrix.CreateScale(ToXna(scale.Value)) *
                                Matrix.CreateFromQuaternion(ToXna(rotation.Value)) *
                                Matrix.CreateTranslation(ToXna(position.Value));

                    channel.Add(new AnimationKeyframe(time, xform));
                }

                animation.Channels.Add(aiChannel.NodeName, channel);
            }

            return animation;
        }

        private static Matrix GetAbsoluteTransform(Node rootNode)
        {
            var xformStack = new Stack<Matrix>();
            var node = rootNode;
            while (node.Parent != null)
            {
                xformStack.Push(ToXna(node.Transform));
                node = node.Parent;
            }

            var xform = Matrix.Identity;
            while (xformStack.Count > 0)
                xform *= xformStack.Pop();
            return xform;
        }

        #region Conversion Helpers

        public static Matrix ToXna(Matrix4x4 matrix)
        {
            var result = Matrix.Identity;

            result.M11 = matrix.A1;
            result.M12 = matrix.B1;
            result.M13 = matrix.C1;
            result.M14 = matrix.D1;

            result.M21 = matrix.A2;
            result.M22 = matrix.B2;
            result.M23 = matrix.C2;
            result.M24 = matrix.D2;

            result.M31 = matrix.A3;
            result.M32 = matrix.B3;
            result.M33 = matrix.C3;
            result.M34 = matrix.D3;

            result.M41 = matrix.A4;
            result.M42 = matrix.B4;
            result.M43 = matrix.C4;
            result.M44 = matrix.D4;

            return result;
        }

        public static IEnumerable<Vector2> ToXna(IEnumerable<Vector2D> vectors)
        {
            foreach (var vector in vectors)
                yield return ToXna(vector);
        }

        public static IEnumerable<Vector3> ToXna(IEnumerable<Vector3D> vectors)
        {
            foreach (var vector in vectors)
                yield return ToXna(vector);
        }

        public static IEnumerable<Vector2> ToXnaTexCoord(IEnumerable<Vector3D> vectors)
        {
            foreach (var vector in vectors)
                yield return new Vector2(vector.X, vector.Y);
        }

        public static Vector2 ToXna(Vector2D vector)
        {
            return new Vector2(vector.X, vector.Y);
        }

        public static Vector3 ToXna(Vector3D vector)
        {
            return new Vector3(vector.X, vector.Y, vector.Z);
        }

        public static Quaternion ToXna(Assimp.Quaternion quaternion)
        {
            return new Quaternion(quaternion.X, quaternion.Y, quaternion.Z, quaternion.W);
        }

        #endregion
    }
}
