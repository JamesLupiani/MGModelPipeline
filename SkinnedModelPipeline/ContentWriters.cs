using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using SkinnedModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkinnedModelPipeline
{
    // This won't be needed once ReflectiveWriter is implemented

    [ContentTypeWriter]
    public class SkinningDataWriter : ContentTypeWriter<SkinningData>
    {
        protected override void Write(ContentWriter output, SkinningData value)
        {
            output.WriteObject(value.AnimationClips);
            output.WriteObject(value.BindPose);
            output.WriteObject(value.InverseBindPose);
            output.WriteObject(value.SkeletonHierarchy);
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return typeof(SkinningDataReader).AssemblyQualifiedName;
        }
    }

    [ContentTypeWriter]
    public class AnimationClipWriter : ContentTypeWriter<AnimationClip>
    {
        protected override void Write(ContentWriter output, AnimationClip value)
        {
            output.WriteObject(value.Duration);
            output.WriteObject(value.Keyframes);
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return typeof(AnimationClipReader).AssemblyQualifiedName;
        }
    }

    [ContentTypeWriter]
    public class KeyframeWriter : ContentTypeWriter<Keyframe>
    {
        protected override void Write(ContentWriter output, Keyframe value)
        {
            output.Write(value.Bone);
            output.WriteObject(value.Time);
            output.Write(value.Transform);
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return typeof(KeyframeReader).AssemblyQualifiedName;
        }
    }
}
