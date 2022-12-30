using StereoKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualScript
{
    public abstract class MatirxWorldObject : WorldObject
    {
        public Matrix MatrixPos;

		protected override void Init()
        {
            MatrixPos = Matrix.TS(new Vec3(0, -1.5f, 0), new Vec3(30, 0.1f, 30));
        }
    }
}
