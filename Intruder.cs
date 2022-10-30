using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenGL
{
    struct intersect_type
    {
        Vector2 vertex;

        int line1, line2;
        double t1, t2;
        bool valid;
        bool used;
    }

    struct segment_type
    {
        Vector2 vertex_start;
        Vector2 vertex_end;
    }

    struct triangle_type
    {
        int seg_ID;
        int corner_ID;
    }
    internal class Intruder
    {
        int SCRN_WDTH = 600;
        int SCRN_HGHT = 400;
        int GridSpacing = 50;

        bool Lft_Btn = false;
        bool Mdl_Btn = false;
        bool Rgt_Btn = false;
        bool Lft_Rgt_Dwn = false;

        int MousePrevX = 0;
        int MousePrevY = 0;
        int MouseX = 0;
        int MouseY = 0;
        int MouseClickX = 0;
        int MouseClickY = 0;

        float[,] InputPoints = new float[300, 2];
        float[,] ControlPoints = new float[600, 2];
        int ControlPointNumber = 0;
        int InputPointNumber = 0;


        float[,] LeftSideLine = new float[600, 2];
        float[,] RightSideLine = new float[600, 2];

        segment_type[] SideLines = new segment_type[1200];
        int SideLineNum;

        intersect_type[] OutlineInterPoints = new intersect_type[300 * 300 * 4];
        int OutlineInterNum;

        int[,] OutlineValidSegmentEnds = new int[300 * 300 * 4,2];
        int OutlineValidSegmentNum;


        int[] OutlineVertIndex = new int[300 * 300 * 4];
        int OutlineVertNum;

        triangle_type[] TriangleList = new triangle_type[300 * 2 + 10];
        int TriangleNum;

        float OutlineDistance = 50.0f;




        int SelectedVertex = -1;
        int SelectedSegment = -1;
        bool IsLineSelMode = false;
        bool CtrPressed = false;

        bool ShowHelp = false;
        bool AllVertShown = false;
        bool ContourVertShown = false;
        bool AllLinesShown = false;
        bool SnapToGrid = false;

        float[] ViewPan = new float[2];

		float Dot2D(float[] v1, float[] v2)
		{
			return v1[0] * v2[0] + v1[1] * v2[1];
		}

		void PerpVectRight(ref float[] v1, float[] v2)
		{
		out[0] = in[1];
		out[1] = -in[0];
		}
		void PerpVectLeft(float* in, float* out)
		{
	out[0] = -in[1];
	out[1] = in[0];
		}

		void ValidateControlLine()
		{
			float[] prev_pt = new float[2];
			float[] cur_dir = new float[2];
			float[] next_dir = new float[2];
			float len1, len2;

			ControlPointNumber = 0;
			TriangleNum = 0;

			if (InputPointNumber < 2)
				return;

			ControlPoints[0,0] = prev_pt[0] = InputPoints[0,0];
			ControlPoints[0,1] = prev_pt[1] = InputPoints[0,1];

			ControlPointNumber++;

			for (int i = 1; i < InputPointNumber; i++)
			{
				//float len1,len2;

				cur_dir[0] = InputPoints[i,0] - prev_pt[0];
				cur_dir[1] = InputPoints[i,1] - prev_pt[1];

				len1 = (float)Math.Sqrt(Dot2D(cur_dir, cur_dir));

				if (len1 < 2.0)
					continue;

				cur_dir[0] /= len1;
				cur_dir[1] /= len1;

				if (i < InputPointNumber - 1)
				{
					next_dir[0] = InputPoints[i + 1,0] - InputPoints[i,0];
					next_dir[1] = InputPoints[i + 1,1] - InputPoints[i,1];

					len2 = (float)Math.Sqrt(Dot2D(next_dir, next_dir));

					if (len2 < 2.0)
						continue;

					next_dir[0] /= len2;
					next_dir[1] /= len2;

					if (Dot2D(next_dir, cur_dir) > 0 && abs(Cross2D(next_dir, cur_dir)) < 0.02)
						continue;


				}

				prev_pt[0] = InputPoints[i,0];
				prev_pt[1] = InputPoints[i,1];

				ControlPoints[ControlPointNumber,0] = InputPoints[i,0];
				ControlPoints[ControlPointNumber,1] = InputPoints[i,1];

				ControlPointNumber++;
			}

			if (ControlPointNumber < 2)
				return;

			float ratio;
			float[] perp = new float[2];

			cur_dir[0] = ControlPoints[2,0] - ControlPoints[1,0];
			cur_dir[1] = ControlPoints[2,1] - ControlPoints[1,1];

			next_dir[0] = ControlPoints[0,0] - ControlPoints[1,0];
			next_dir[1] = ControlPoints[0,1] - ControlPoints[1,1];

			PerpVectRight(cur_dir, perp);

			Norm2D(perp);

			len1 = sqrt(Dot2D(next_dir, next_dir));

			Norm2D(next_dir);

			perp[0] *= OutlineDistance;
			perp[1] *= OutlineDistance;

			ratio = abs(Dot2D(perp, next_dir));

			if (ratio * 1.1 > len1)
			{
				next_dir[0] *= ratio * 1.1;
				next_dir[1] *= ratio * 1.1;

				ControlPoints[0,0] = ControlPoints[1,0] + next_dir[0];
				ControlPoints[0,1] = ControlPoints[1,1] + next_dir[1];
			}

			/***********/

			cur_dir[0] = ControlPoints[ControlPointNumber - 3,0]
				- ControlPoints[ControlPointNumber - 2,0];
			cur_dir[1] = ControlPoints[ControlPointNumber - 3,1]
				- ControlPoints[ControlPointNumber - 2,1];

			next_dir[0] = ControlPoints[ControlPointNumber - 1,0]
				- ControlPoints[ControlPointNumber - 2,0];
			next_dir[1] = ControlPoints[ControlPointNumber - 1,1]
				- ControlPoints[ControlPointNumber - 2,1];

			PerpVectRight(cur_dir, perp);

			Norm2D(perp);

			len1 = sqrt(Dot2D(next_dir, next_dir));

			Norm2D(next_dir);

			perp[0] *= OutlineDistance;
			perp[1] *= OutlineDistance;

			ratio = abs(Dot2D(perp, next_dir));

			if (ratio * 1.1 > len1)
			{
				next_dir[0] *= ratio * 1.1;
				next_dir[1] *= ratio * 1.1;

				ControlPoints[ControlPointNumber - 1,0] =
					ControlPoints[ControlPointNumber - 2,0] + next_dir[0];
				ControlPoints[ControlPointNumber - 1,1] =
					ControlPoints[ControlPointNumber - 2,1] + next_dir[1];
			}

		}

	}
}
