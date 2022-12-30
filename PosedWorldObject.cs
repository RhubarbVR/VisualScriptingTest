using StereoKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualScript
{
    public abstract class PosedWorldObject : WorldObject
    {
        public Pose Pos;

		protected override void Init()
        {
            Pos = Pose.Identity;
        }
    }
}
