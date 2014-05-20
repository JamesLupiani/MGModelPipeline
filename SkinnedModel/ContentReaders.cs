using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkinnedModel
{
    // This won't be needed once ReflectiveWriter is implemented

    public class SkinningDataReader : ContentTypeReader<SkinningData>
    {
        protected override SkinningData Read(ContentReader input, SkinningData existingInstance)
        {
            var animationClips = input.ReadObject<Dictionary<string, AnimationClip>>();
            var bindPose = input.ReadObject<List<Matrix>>();
            var inverseBindPose = input.ReadObject<List<Matrix>>();
            var skeletonHierarchy = input.ReadObject<List<int>>();

            return new SkinningData(animationClips, bindPose, inverseBindPose, skeletonHierarchy);
        }
    }

    public class AnimationClipReader : ContentTypeReader<AnimationClip>
    {
        protected override AnimationClip Read(ContentReader input, AnimationClip existingInstance)
        {
            var duration = input.ReadObject<TimeSpan>();
            var keyframes = input.ReadObject<List<Keyframe>>();

            return new AnimationClip(duration, keyframes);
        }
    }

    public class KeyframeReader : ContentTypeReader<Keyframe>
    {
        protected override Keyframe Read(ContentReader input, Keyframe existingInstance)
        {
            var bone = input.ReadInt32();
            var time = input.ReadObject<TimeSpan>();
            var xform = input.ReadMatrix();

            return new Keyframe(bone, time, xform);
        }
    }
}
