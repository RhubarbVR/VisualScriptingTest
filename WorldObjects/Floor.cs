using StereoKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualScript
{
    public sealed class Floor : MatirxWorldObject
    {
        public Material floorMaterial;

		protected override void Step(float dt)
        {
            if (SK.System.displayType == Display.Opaque) {
				Default.MeshCube.Draw(floorMaterial, MatrixPos);
			}
		}

		protected override void Init()
        {
            floorMaterial = new Material(Shader.FromFile("floor.hlsl"))
            {
                Transparency = Transparency.Blend
            };
            base.Init();
        }
    }
}
