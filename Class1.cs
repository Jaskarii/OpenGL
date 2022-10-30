# include <windows.h>
# include <stdio.h>
# include <math.h>
# include <gl\gl.h>

typedef struct
{
	HGLRC hRC;
HDC hDC;
HWND hWnd;
HINSTANCE hInstance;
}
win_state_type;

win_state_type win_state = { NULL, NULL, NULL, NULL };



RECT WindowRect;

int SCRN_WDTH = 600;
int SCRN_HGHT = 400;
int GridSpacing = 50;

bool Lft_Btn = false;
bool Mdl_Btn = false;
bool Rgt_Btn = false;
bool Lft_Rgt_Dwn = false;

# include "ms_print.h"

int MousePrevX = 0;
int MousePrevY = 0;
int MouseX = 0;
int MouseY = 0;
int MouseClickX = 0;
int MouseClickY = 0;

//bool EditMode = true;
//bool Loofasz = false;

typedef struct
{
	float vertex_start[2];
float vertex_end[2];
}
segment_type;

typedef struct
{
	float vertex[2];

int line1, line2;
double t1, t2;

bool valid;
bool used;
}
intersect_type;

typedef struct
{
	int seg_ID;

//float corner[2];
int corner_ID;
}
triangle_type;



/*****************/

#define MAX_POINTS		300

float InputPoints[MAX_POINTS][2] = { };
float ControlPoints[MAX_POINTS * 2][2] = { };
int ControlPointNumber = 0;
int InputPointNumber = 0;


float LeftSideLine[MAX_POINTS * 2][2];
float RightSideLine[MAX_POINTS * 2][2];
//float SideLines[MAX_POINTS*4][2];
//int   SideLineNum;

segment_type SideLines[MAX_POINTS * 4];
int SideLineNum;

intersect_type OutlineInterPoints[MAX_POINTS * MAX_POINTS * 4];
int OutlineInterNum;

int OutlineValidSegmentEnds[MAX_POINTS * MAX_POINTS * 4][2];
int OutlineValidSegmentNum;


int OutlineVertIndex[MAX_POINTS * MAX_POINTS * 4];
int OutlineVertNum;

triangle_type TriangleList[MAX_POINTS * 2 + 10];
int TriangleNum;

float OutlineDistance = 50.0;


#define WIND_CLASS_NAME		"Outline 1.0" 
LRESULT CALLBACK WndProc(HWND hWnd, UINT uMsg, WPARAM wParam, LPARAM lParam);

/***************************/

int SelectedVertex = -1;
int SelectedSegment = -1;
bool IsLineSelMode = false;
bool CtrPressed = false;

bool ShowHelp = false;
bool AllVertShown = false;
bool ContourVertShown = false;
bool AllLinesShown = false;
bool SnapToGrid = false;

float ViewPan[2] = { };

/*****************************/

void vekt_szorz(float* a, float* b, float* out)
{
	out[0] = a[1] * b[2] - a[2] * b[1];
	out[1] = a[2] * b[0] - a[0] * b[2];
	out[2] = a[0] * b[1] - a[1] * b[0];
}
float skal_szorz(float* a, float* b)
{
	return a[0] * b[0] + a[1] * b[1] + a[2] * b[2];
}
/***********/
void PerpVectRight(float* in, float* out)
{
	out[0] = in[1];
	out[1] = -in[0];
}
void PerpVectLeft(float* in, float* out)
{
	out[0] = -in[1];
	out[1] = in[0];
}
float Dot2D(float* v1, float* v2)
{
	return v1[0] * v2[0] + v1[1] * v2[1];
}

void Norm2D(float* v)
{
	float l = sqrt(Dot2D(v, v));

	if (abs(l) > 0.0000001)
	{
		v[0] /= l;
		v[1] /= l;
	}
}
float Cross2D(float* v1, float* v2)
{
	return v1[0] * v2[1] - v2[0] * v1[1];
}
void IntersectSegment(segment_type* seg1, segment_type* seg2,
	double* t1, double* t2, float* P)
{
	double x4_x3, x2_x1;
	double y4_y3, y2_y1;

	x4_x3 = seg2->vertex_end[0] - seg2->vertex_start[0];
	x2_x1 = seg1->vertex_end[0] - seg1->vertex_start[0];

	y4_y3 = seg2->vertex_end[1] - seg2->vertex_start[1];
	y2_y1 = seg1->vertex_end[1] - seg1->vertex_start[1];

	*t1 = x4_x3 * (seg1->vertex_start[1] - seg2->vertex_start[1])
		- y4_y3 * (seg1->vertex_start[0] - seg2->vertex_start[0]);

	if (abs(y4_y3 * x2_x1 - x4_x3 * y2_y1) < 0.01)
	{
		*t1 = -1;
		return;
	}

	*t1 /= y4_y3 * x2_x1 - x4_x3 * y2_y1;

	*t2 = x2_x1 * (seg1->vertex_start[1] - seg2->vertex_start[1])
		- y2_y1 * (seg1->vertex_start[0] - seg2->vertex_start[0]);

	if (abs(y4_y3 * x2_x1 - x4_x3 * y2_y1) < 0.01)
	{
		*t1 = -1;
		return;
	}

	*t2 /= y4_y3 * x2_x1 - x4_x3 * y2_y1;

	P[0] = seg1->vertex_start[0] + (*t1) * x2_x1;
	P[1] = seg1->vertex_start[1] + (*t1) * y2_y1;
}


bool Poin_On_Poly(int sides, float* point, float* poly_pts, float* norm)
{
	float v1[3], v2[3], n[3];
	float skal;

	for (int i = 0; i < sides; i++)
	{
		v1[0] = poly_pts[i * 3 + 0] - point[0];
		v1[1] = poly_pts[i * 3 + 1] - point[1];
		v1[2] = poly_pts[i * 3 + 2] - point[2];

		v2[0] = poly_pts[((i + 1) % sides) * 3 + 0] - point[0];
		v2[1] = poly_pts[((i + 1) % sides) * 3 + 1] - point[1];
		v2[2] = poly_pts[((i + 1) % sides) * 3 + 2] - point[2];

		vekt_szorz(v1, v2, n);
		skal = skal_szorz(n, norm);

		if (skal > -0.1)
			return 0;
	}

	return 1;
}

bool Point_On_trapez(int side_seg, int contol_seg, float* pt)
{
	float point[12];
	float pt3[3] = { pt[0], pt[1], 0 };
	float dir1[2], dir2[2];
	float norm[] = { 0, 0, -1 };

	dir1[0] = SideLines[side_seg].vertex_end[0] - SideLines[side_seg].vertex_start[0];
	dir1[1] = SideLines[side_seg].vertex_end[1] - SideLines[side_seg].vertex_start[1];

	dir2[0] = ControlPoints[contol_seg + 1][0] - ControlPoints[contol_seg][0];
	dir2[1] = ControlPoints[contol_seg + 1][1] - ControlPoints[contol_seg][1];

	point[0 * 3 + 0] = SideLines[side_seg].vertex_start[0];
	point[0 * 3 + 1] = SideLines[side_seg].vertex_start[1];
	point[0 * 3 + 2] = 0;

	point[1 * 3 + 0] = SideLines[side_seg].vertex_end[0];
	point[1 * 3 + 1] = SideLines[side_seg].vertex_end[1];
	point[1 * 3 + 2] = 0;

	if (Dot2D(dir1, dir2) > 0)
	{
		point[2 * 3 + 0] = ControlPoints[contol_seg + 1][0];
		point[2 * 3 + 1] = ControlPoints[contol_seg + 1][1];
		point[2 * 3 + 2] = 0;

		point[3 * 3 + 0] = ControlPoints[contol_seg][0];
		point[3 * 3 + 1] = ControlPoints[contol_seg][1];
		point[3 * 3 + 2] = 0;
	}
	else
	{
		point[3 * 3 + 0] = ControlPoints[contol_seg + 1][0];
		point[3 * 3 + 1] = ControlPoints[contol_seg + 1][1];
		point[3 * 3 + 2] = 0;

		point[2 * 3 + 0] = ControlPoints[contol_seg][0];
		point[2 * 3 + 1] = ControlPoints[contol_seg][1];
		point[2 * 3 + 2] = 0;
	}

	return Poin_On_Poly(4, pt3, point, norm);
}
bool Point_On_Tri(int tri_ID, float* pt)
{
	float point[9];
	float pt3[3] = { pt[0], pt[1], 0 };
	float norm[] = { 0, 0, -1 };

	point[0 * 3 + 0] = ControlPoints[TriangleList[tri_ID].corner_ID][0];
	point[0 * 3 + 1] = ControlPoints[TriangleList[tri_ID].corner_ID][1];
	point[0 * 3 + 2] = 0;

	point[1 * 3 + 0] = SideLines[TriangleList[tri_ID].seg_ID].vertex_start[0];
	point[1 * 3 + 1] = SideLines[TriangleList[tri_ID].seg_ID].vertex_start[1];
	point[1 * 3 + 2] = 0;

	point[2 * 3 + 0] = SideLines[TriangleList[tri_ID].seg_ID].vertex_end[0];
	point[2 * 3 + 1] = SideLines[TriangleList[tri_ID].seg_ID].vertex_end[1];
	point[2 * 3 + 2] = 0;

	return Poin_On_Poly(3, pt3, point, norm);
}
void SortIndex(int* index, float* val, int max)
{
	for (int i = 0; i < max - 1; i++)
	{
		for (int k = i + 1; k < max; k++)
		{
			int store;
			float f;

			if (val[k] < val[i])
			{
				store = index[i];
				index[i] = index[k];
				index[k] = store;

				f = val[i];
				val[i] = val[k];
				val[k] = f;
			}
		}
	}
}
/***************************/

void ValidateControlLine()
{
	float cur_dir[2], next_dir[2];
	float prev_pt[2];
	float len1, len2;

	ControlPointNumber = 0;
	TriangleNum = 0;

	if (InputPointNumber < 2)
		return;

	ControlPoints[0][0] = prev_pt[0] = InputPoints[0][0];
	ControlPoints[0][1] = prev_pt[1] = InputPoints[0][1];

	ControlPointNumber++;

	for (int i = 1; i < InputPointNumber; i++)
	{
		//float len1,len2;

		cur_dir[0] = InputPoints[i][0] - prev_pt[0];
		cur_dir[1] = InputPoints[i][1] - prev_pt[1];

		len1 = sqrt(Dot2D(cur_dir, cur_dir));

		if (len1 < 2.0)
			continue;

		cur_dir[0] /= len1;
		cur_dir[1] /= len1;

		if (i < InputPointNumber - 1)
		{
			next_dir[0] = InputPoints[i + 1][0] - InputPoints[i][0];
			next_dir[1] = InputPoints[i + 1][1] - InputPoints[i][1];

			len2 = sqrt(Dot2D(next_dir, next_dir));

			if (len2 < 2.0)
				continue;

			next_dir[0] /= len2;
			next_dir[1] /= len2;

			if (Dot2D(next_dir, cur_dir) > 0 && abs(Cross2D(next_dir, cur_dir)) < 0.02)
				continue;


		}

		prev_pt[0] = InputPoints[i][0];
		prev_pt[1] = InputPoints[i][1];

		ControlPoints[ControlPointNumber][0] = InputPoints[i][0];
		ControlPoints[ControlPointNumber][1] = InputPoints[i][1];

		ControlPointNumber++;
	}

	if (ControlPointNumber < 2)
		return;

	float ratio;
	float perp[2];

	cur_dir[0] = ControlPoints[2][0] - ControlPoints[1][0];
	cur_dir[1] = ControlPoints[2][1] - ControlPoints[1][1];

	next_dir[0] = ControlPoints[0][0] - ControlPoints[1][0];
	next_dir[1] = ControlPoints[0][1] - ControlPoints[1][1];

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

		ControlPoints[0][0] = ControlPoints[1][0] + next_dir[0];
		ControlPoints[0][1] = ControlPoints[1][1] + next_dir[1];
	}

	/***********/

	cur_dir[0] = ControlPoints[ControlPointNumber - 3][0]
		- ControlPoints[ControlPointNumber - 2][0];
	cur_dir[1] = ControlPoints[ControlPointNumber - 3][1]
		- ControlPoints[ControlPointNumber - 2][1];

	next_dir[0] = ControlPoints[ControlPointNumber - 1][0]
		- ControlPoints[ControlPointNumber - 2][0];
	next_dir[1] = ControlPoints[ControlPointNumber - 1][1]
		- ControlPoints[ControlPointNumber - 2][1];

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

		ControlPoints[ControlPointNumber - 1][0] =
			ControlPoints[ControlPointNumber - 2][0] + next_dir[0];
		ControlPoints[ControlPointNumber - 1][1] =
			ControlPoints[ControlPointNumber - 2][1] + next_dir[1];
	}

}

void CalcTrapezoidCorner(float* vert, float* perp_edge_dir, float* tr_dir,
	float distance, float* tr_corner)
{
	float dot_product = Dot2D(tr_dir, perp_edge_dir);

	tr_corner[0] = vert[0] + distance / dot_product * tr_dir[0];
	tr_corner[1] = vert[1] + distance / dot_product * tr_dir[1];
}

void CalculateRightLines()
{
	float trap_dir[2], trap_dir_2[2];
	float control_dir1[2];
	float control_dir2[2];
	float perp[2];
	bool is_new_triangle;

	if (ControlPointNumber < 2)
		return;

	control_dir1[0] = ControlPoints[1][0] - ControlPoints[0][0];
	control_dir1[1] = ControlPoints[1][1] - ControlPoints[0][1];

	PerpVectRight(control_dir1, perp);
	Norm2D(perp);

	RightSideLine[0][0] = ControlPoints[0][0] + perp[0] * OutlineDistance;
	RightSideLine[0][1] = ControlPoints[0][1] + perp[1] * OutlineDistance;

	///////////

	control_dir1[0] = ControlPoints[ControlPointNumber - 1][0]
		- ControlPoints[ControlPointNumber - 2][0];

	control_dir1[1] = ControlPoints[ControlPointNumber - 1][1]
		- ControlPoints[ControlPointNumber - 2][1];

	PerpVectRight(control_dir1, perp);
	Norm2D(perp);

	RightSideLine[2 * ControlPointNumber - 3][0] =
		ControlPoints[ControlPointNumber - 1][0] + perp[0] * OutlineDistance;

	RightSideLine[2 * ControlPointNumber - 3][1] =
		ControlPoints[ControlPointNumber - 1][1] + perp[1] * OutlineDistance;

	for (int i = 1; i < ControlPointNumber - 1; i++)
	{
		is_new_triangle = false;

		control_dir1[0] = ControlPoints[i - 1][0] - ControlPoints[i][0];
		control_dir1[1] = ControlPoints[i - 1][1] - ControlPoints[i][1];

		control_dir2[0] = ControlPoints[i + 1][0] - ControlPoints[i][0];
		control_dir2[1] = ControlPoints[i + 1][1] - ControlPoints[i][1];

		Norm2D(control_dir1);
		Norm2D(control_dir2);

		trap_dir_2[0] = trap_dir[0] = control_dir1[0] + control_dir2[0];
		trap_dir_2[1] = trap_dir[1] = control_dir1[1] + control_dir2[1];

		PerpVectRight(control_dir2, perp);
		Norm2D(perp);

		if (Dot2D(control_dir1, perp) > 0)
		{
			trap_dir[0] = perp[0];
			trap_dir[1] = perp[1];
		}
		else if (Dot2D(control_dir2, trap_dir) > 1.0)
		{
			trap_dir[0] = perp[0] - control_dir2[0];
			trap_dir[1] = perp[1] - control_dir2[1];

			is_new_triangle = true;
		}

		CalcTrapezoidCorner(ControlPoints[i], perp, trap_dir,
			OutlineDistance, RightSideLine[i * 2]);

		if (is_new_triangle)
		{
			SideLines[ControlPointNumber * 2 + TriangleNum].vertex_end[0]
				= RightSideLine[i * 2][0];
			SideLines[ControlPointNumber * 2 + TriangleNum].vertex_end[1]
				= RightSideLine[i * 2][1];
		}
		////

		PerpVectLeft(control_dir1, perp);
		Norm2D(perp);

		if (Dot2D(control_dir2, perp) > 0)
		{
			trap_dir_2[0] = perp[0];
			trap_dir_2[1] = perp[1];
		}
		else if (Dot2D(control_dir1, trap_dir_2) > 1.0)
		{
			trap_dir_2[0] = perp[0] - control_dir1[0];
			trap_dir_2[1] = perp[1] - control_dir1[1];
		}

		CalcTrapezoidCorner(ControlPoints[i], perp, trap_dir_2,
			OutlineDistance, RightSideLine[i * 2 - 1]);

		if (is_new_triangle)
		{
			SideLines[ControlPointNumber * 2 + TriangleNum].vertex_start[0]
				= RightSideLine[i * 2 - 1][0];
			SideLines[ControlPointNumber * 2 + TriangleNum].vertex_start[1]
				= RightSideLine[i * 2 - 1][1];

			TriangleList[TriangleNum].corner_ID = i;
			TriangleList[TriangleNum].seg_ID = ControlPointNumber * 2 + TriangleNum;

			TriangleNum++;
		}
	}
}
void CalculateLeftLines()
{
	float trap_dir[2], trap_dir_2[2];
	float control_dir1[2];
	float control_dir2[2];
	float perp[2];

	bool is_new_triangle;

	if (ControlPointNumber < 2)
		return;

	control_dir1[0] = ControlPoints[1][0] - ControlPoints[0][0];
	control_dir1[1] = ControlPoints[1][1] - ControlPoints[0][1];

	PerpVectLeft(control_dir1, perp);
	Norm2D(perp);

	LeftSideLine[0][0] = ControlPoints[0][0] + perp[0] * OutlineDistance;
	LeftSideLine[0][1] = ControlPoints[0][1] + perp[1] * OutlineDistance;

	///////////

	control_dir1[0] = ControlPoints[ControlPointNumber - 1][0]
		- ControlPoints[ControlPointNumber - 2][0];

	control_dir1[1] = ControlPoints[ControlPointNumber - 1][1]
		- ControlPoints[ControlPointNumber - 2][1];

	PerpVectLeft(control_dir1, perp);
	Norm2D(perp);

	LeftSideLine[2 * ControlPointNumber - 3][0] =
		ControlPoints[ControlPointNumber - 1][0] + perp[0] * OutlineDistance;

	LeftSideLine[2 * ControlPointNumber - 3][1] =
		ControlPoints[ControlPointNumber - 1][1] + perp[1] * OutlineDistance;

	for (int i = 1; i < ControlPointNumber - 1; i++)
	{
		is_new_triangle = false;

		control_dir1[0] = ControlPoints[i][0] - ControlPoints[i - 1][0];
		control_dir1[1] = ControlPoints[i][1] - ControlPoints[i - 1][1];

		control_dir2[0] = ControlPoints[i][0] - ControlPoints[i + 1][0];
		control_dir2[1] = ControlPoints[i][1] - ControlPoints[i + 1][1];

		Norm2D(control_dir1);
		Norm2D(control_dir2);

		trap_dir_2[0] = trap_dir[0] = control_dir1[0] + control_dir2[0];
		trap_dir_2[1] = trap_dir[1] = control_dir1[1] + control_dir2[1];

		PerpVectRight(control_dir2, perp);
		Norm2D(perp);

		if (Dot2D(control_dir1, perp) < 0)
		{
			trap_dir[0] = perp[0];
			trap_dir[1] = perp[1];
		}
		else if (Dot2D(control_dir2, trap_dir) > 1.0)
		{
			trap_dir[0] = perp[0] + control_dir2[0];
			trap_dir[1] = perp[1] + control_dir2[1];

			is_new_triangle = true;
		}

		CalcTrapezoidCorner(ControlPoints[i], perp, trap_dir,
			OutlineDistance, LeftSideLine[i * 2]);

		if (is_new_triangle)
		{
			SideLines[ControlPointNumber * 2 + TriangleNum].vertex_start[0]
				= LeftSideLine[i * 2][0];
			SideLines[ControlPointNumber * 2 + TriangleNum].vertex_start[1]
				= LeftSideLine[i * 2][1];
		}
		////

		PerpVectLeft(control_dir1, perp);
		Norm2D(perp);

		if (Dot2D(control_dir2, perp) < 0)
		{
			trap_dir_2[0] = perp[0];
			trap_dir_2[1] = perp[1];
		}
		else if (Dot2D(control_dir1, trap_dir_2) > 1.0)
		{
			trap_dir_2[0] = perp[0] + control_dir1[0];
			trap_dir_2[1] = perp[1] + control_dir1[1];
		}

		CalcTrapezoidCorner(ControlPoints[i], perp, trap_dir_2,
			OutlineDistance, LeftSideLine[i * 2 - 1]);

		if (is_new_triangle)
		{
			SideLines[ControlPointNumber * 2 + TriangleNum].vertex_end[0]
				= LeftSideLine[i * 2 - 1][0];
			SideLines[ControlPointNumber * 2 + TriangleNum].vertex_end[1]
				= LeftSideLine[i * 2 - 1][1];

			TriangleList[TriangleNum].corner_ID = i;
			TriangleList[TriangleNum].seg_ID = ControlPointNumber * 2 + TriangleNum;

			TriangleNum++;
		}
	}
}


void UpdateSideLineBuf()
{
	SideLineNum = 0;

	if (ControlPointNumber < 2)
		return;

	for (int i = 0; i < ControlPointNumber * 2 - 2; i += 2)
	{
		SideLines[SideLineNum].vertex_start[0] = RightSideLine[i][0];
		SideLines[SideLineNum].vertex_start[1] = RightSideLine[i][1];

		SideLines[SideLineNum].vertex_end[0] = RightSideLine[i + 1][0];
		SideLines[SideLineNum].vertex_end[1] = RightSideLine[i + 1][1];
		SideLineNum++;

		SideLines[SideLineNum].vertex_start[0] = LeftSideLine[i + 1][0];
		SideLines[SideLineNum].vertex_start[1] = LeftSideLine[i + 1][1];

		SideLines[SideLineNum].vertex_end[0] = LeftSideLine[i][0];
		SideLines[SideLineNum].vertex_end[1] = LeftSideLine[i][1];
		SideLineNum++;
	}

	SideLines[SideLineNum].vertex_start[0] = LeftSideLine[0][0];
	SideLines[SideLineNum].vertex_start[1] = LeftSideLine[0][1];

	SideLines[SideLineNum].vertex_end[0] = RightSideLine[0][0];
	SideLines[SideLineNum].vertex_end[1] = RightSideLine[0][1];
	SideLineNum++;

	SideLines[SideLineNum].vertex_start[0] = RightSideLine[2 * ControlPointNumber - 3][0];
	SideLines[SideLineNum].vertex_start[1] = RightSideLine[2 * ControlPointNumber - 3][1];

	SideLines[SideLineNum].vertex_end[0] = LeftSideLine[2 * ControlPointNumber - 3][0];
	SideLines[SideLineNum].vertex_end[1] = LeftSideLine[2 * ControlPointNumber - 3][1];
	SideLineNum++;

	SideLineNum += TriangleNum;
}
void CalculateInterPoints()
{
	double t1, t2;

	OutlineInterNum = 0;

	if (SideLineNum < 4)
		return;


	for (int i = 0; i < SideLineNum - 1; i++)
	{
		for (int k = i + 1; k < SideLineNum; k++)
		{
			IntersectSegment(SideLines + i, SideLines + k, &t1, &t2,
				OutlineInterPoints[OutlineInterNum].vertex);

			if (t1 >= -0.00001 && t1 <= 1.00001
				&& t2 >= -0.00001 && t2 <= 1.00001)
			{
				OutlineInterPoints[OutlineInterNum].line1 = i;
				OutlineInterPoints[OutlineInterNum].line2 = k;

				OutlineInterPoints[OutlineInterNum].t1 = t1;
				OutlineInterPoints[OutlineInterNum].t2 = t2;

				OutlineInterNum++;
			}
		}
	}
}
bool IsValidPoint(float* p, int line1, int line2)
{
	SideLineNum -= TriangleNum;

	bool valid = true;

	int k = 0;
	int end = SideLineNum - 2;

	int control_seg, side_seg;

	if (SideLineNum - 2 == line1 || SideLineNum - 2 == line2)
		k = 2;

	if (SideLineNum - 1 == line1 || SideLineNum - 1 == line2)
		end = SideLineNum - 4;

	for (; k < end; k++)
	{
		if (k == line1 || k == line2)
			continue;

		control_seg = k / 2;
		side_seg = k;

		if (Point_On_trapez(side_seg, control_seg, p))
		{
			valid = false;
			//break;
			goto END_OF_VALID_POINT;
		}
	}

	for (k = 0; k < TriangleNum; k++)
	{
		if (SideLineNum + k == line1 || SideLineNum + k == line2)
			continue;

		if (Point_On_Tri(k, p))
		{
			valid = false;
			break;
		}
	}

END_OF_VALID_POINT:

	SideLineNum += TriangleNum;

	return valid;
}
void ValidateInterPoints()
{
	for (int i = 0; i < OutlineInterNum; i++)
	{
		OutlineInterPoints[i].used = false;

		OutlineInterPoints[i].valid = IsValidPoint(OutlineInterPoints[i].vertex,
			OutlineInterPoints[i].line1,
			OutlineInterPoints[i].line2);
	}
}


void SortPointsOnLine(int* list, int seg, int max)
{
	float t[2 * MAX_POINTS + 10];

	for (int i = 0; i < max; i++)
	{
		if (OutlineInterPoints[list[i]].line1 == seg)
			t[i] = OutlineInterPoints[list[i]].t1;
		else if (OutlineInterPoints[list[i]].line2 == seg)
			t[i] = OutlineInterPoints[list[i]].t2;
	}

	SortIndex(list, t, max);
}
void ValidateSegments()
{
	int Line[2 * MAX_POINTS + 10];
	int index;
	bool valid;

	OutlineValidSegmentNum = 0;

	for (int i = 0; i < SideLineNum; i++)
	{
		int j = 0;

		index = 0;
		for (int k = 0; k < OutlineInterNum; k++)
		{
			if ((OutlineInterPoints[k].line1 == i || OutlineInterPoints[k].line2 == i)
				&& OutlineInterPoints[k].valid)
				Line[index++] = k;
		}

		SortPointsOnLine(Line, i, index);
		//val = true;

		while (j < index - 1)
		{
			valid = true;

			for (int k = 1; k < 10; k++)
			{
				float p[2];

				p[0] = OutlineInterPoints[Line[j + 1]].vertex[0] * k / 10.0 +
					OutlineInterPoints[Line[j]].vertex[0] * (10.0 - k) / 10.0;

				p[1] = OutlineInterPoints[Line[j + 1]].vertex[1] * k / 10.0 +
					OutlineInterPoints[Line[j]].vertex[1] * (10.0 - k) / 10.0;

				if (!IsValidPoint(p, i, i))
				{
					valid = false;
					break;
				}
			}

			if (valid)//IsValidPoint(p,i,i) )
			{
				OutlineValidSegmentEnds[OutlineValidSegmentNum][0] = Line[j];
				OutlineValidSegmentEnds[OutlineValidSegmentNum][1] = Line[j + 1];

				OutlineValidSegmentNum++;
			}

			j++;

			/*float p[2];

			p[0] =	OutlineInterPoints[Line[j+1]].vertex[0]*1.0/3.0 +
					OutlineInterPoints[Line[j]].vertex[0]*2.0/3.0;

			p[1] =	OutlineInterPoints[Line[j+1]].vertex[1]*1.0/3.0 +
					OutlineInterPoints[Line[j]].vertex[1]*2.0/3.0;

			if( IsValidPoint(p,i,i) )
			{
				OutlineValidSegmentEnds[OutlineValidSegmentNum][0] = Line[j];
				OutlineValidSegmentEnds[OutlineValidSegmentNum][1] = Line[j+1];

				OutlineValidSegmentNum ++;
			}

			j+=1;*/
		}

	}
}
int debug_foo;

void MarchingEdges()
{
	int valid_index = 0;
	int new_index = 0;
	int cur_segment = 0, new_segment;
	//float cur_t = 0,new_t;
	float dist, min_dist;

	float current_dir[2];
	float new_dir[2];
	float perp[2];

	bool is_right = false;

	OutlineVertNum = 0;

	if (ControlPointNumber < 2)
		return;

	while (!OutlineInterPoints[valid_index].valid)
		valid_index++;

	cur_segment = OutlineInterPoints[valid_index].line2;
	OutlineInterPoints[valid_index].used = true;

	OutlineVertIndex[OutlineVertNum] = valid_index;
	OutlineVertNum++;

	current_dir[0] = SideLines[cur_segment].vertex_end[0]
		- SideLines[cur_segment].vertex_start[0];

	current_dir[1] = SideLines[cur_segment].vertex_end[1]
		- SideLines[cur_segment].vertex_start[1];

	PerpVectRight(current_dir, perp);
	Norm2D(perp);

	debug_foo = 0;

	while (1)
	{
		int k;

		glPushMatrix();
		glLoadIdentity();
		glPrint(10, 940 - debug_foo, "current: %d, line1: %d, line2: %d",
			cur_segment,
			OutlineInterPoints[valid_index].line1,
			OutlineInterPoints[valid_index].line2);
		glPopMatrix();
		debug_foo += 20;

		if (OutlineInterPoints[valid_index].line1 == cur_segment)
		{
			new_segment = OutlineInterPoints[valid_index].line2;
			//cur_t = OutlineInterPoints[valid_index].t2;
		}

		else
		{
			new_segment = OutlineInterPoints[valid_index].line1;
			//cur_t = OutlineInterPoints[valid_index].t1;
		}

		PerpVectRight(current_dir, perp);
		Norm2D(perp);



		glBegin(GL_LINES);

		glColor4f(0, 0, 1, 1);

		glVertex2fv(OutlineInterPoints[valid_index].vertex);
		glVertex2f(OutlineInterPoints[valid_index].vertex[0] + perp[0] * 30,
			OutlineInterPoints[valid_index].vertex[1] + perp[1] * 30);

		glEnd();




		min_dist = dist = 10000000.0;

		is_right = false;

		for (k = 0; k < OutlineInterNum; k++)
		{
			if (k == valid_index || !OutlineInterPoints[k].valid)
				continue;

			if (new_segment == OutlineInterPoints[k].line1
				|| new_segment == OutlineInterPoints[k].line2)
			{
				new_dir[0] = OutlineInterPoints[k].vertex[0]
					- OutlineInterPoints[valid_index].vertex[0];
				new_dir[1] = OutlineInterPoints[k].vertex[1]
					- OutlineInterPoints[valid_index].vertex[1];

				dist = Dot2D(perp, new_dir);

				if (dist > 0 && !is_right)
				{
					is_right = true;
					min_dist = dist;
				}

				if (is_right && dist < 0)
					continue;

				if (abs(min_dist) > abs(dist))
				{
					min_dist = dist;
					new_index = k;

					current_dir[0] = new_dir[0];
					current_dir[1] = new_dir[1];
				}
			}
		}

		valid_index = new_index;
		cur_segment = new_segment;

		//current_dir[0] = new_dir[0];
		//current_dir[1] = new_dir[1];

		glPushMatrix();
		glLoadIdentity();
		glPrint(200, 700 - OutlineVertNum * 20, "min_dist: %f", min_dist);
		glPopMatrix();

		glBegin(GL_LINES);

		glColor4f(1, 0, 1, 1);

		glVertex2f(OutlineInterPoints[valid_index].vertex[0] + 2,
			OutlineInterPoints[valid_index].vertex[1] + 2);
		glVertex2f(OutlineInterPoints[valid_index].vertex[0] + current_dir[0] * 0.3 + 2,
			OutlineInterPoints[valid_index].vertex[1] + current_dir[1] * 0.3 + 2);

		glEnd();


		OutlineVertIndex[OutlineVertNum] = valid_index;
		OutlineVertNum++;

		if (OutlineVertNum > 20)//OutlineInterPoints[valid_index].used )
			break;

		OutlineInterPoints[valid_index].used = true;
	}
}
/***************************/

void KeyboardDownHandler(int key)
{
	switch (key)
	{
		case 'R':
			InputPointNumber = 0;
			break;

		case VK_F1:
			ShowHelp ^= 1;
			break;

		case 'S':
			SnapToGrid ^= 1;
			break;

		case 'C':
			ContourVertShown ^= 1;
			break;
		case 'V':
			AllVertShown ^= 1; ;
			break;
		case 'L':
			AllLinesShown ^= 1; ;
			break;

		case VK_ADD:
			if (OutlineDistance < 200)
				OutlineDistance += 5;
			break;

		case VK_SUBTRACT:
			if (OutlineDistance > 10)
				OutlineDistance -= 5;
			break;

		case VK_CONTROL:
			CtrPressed = true;
			break;

		case VK_SPACE:
			IsLineSelMode = true;
			break;
	}
}
void KeyboardUpHandler(int key)
{
	switch (key)
	{
		case VK_CONTROL:
			CtrPressed = false;
			break;

		case VK_SPACE:
			IsLineSelMode = false;
			break;
	}
}

void Snap(int x, int y, int* sx, int* sy)
{
	*sx = floor((float)(x / (float)GridSpacing) + 0.5) * GridSpacing;
	*sy = floor((float)(y / (float)GridSpacing) + 0.5) * GridSpacing;
}
void SelectPoint(int x, int y)
{
	SelectedSegment = -1;
	SelectedVertex = -1;

	x -= ViewPan[0];
	y -= ViewPan[1];

	for (int i = 0; i < InputPointNumber; i++)
	{
		float disp[2] = { x - InputPoints[i][0], y - InputPoints[i][1] };

		float distance = sqrt(Dot2D(disp, disp));

		if (distance < 5)
		{
			SelectedVertex = i;
			break;
		}
	}
}
void SelectSegment(int x, int y)
{
	float seg_dir[2];
	float delta[2];
	float perp[2];
	float len;

	SelectedSegment = -1;
	SelectedVertex = -1;

	if (!IsLineSelMode)
		return;

	x -= ViewPan[0];
	y -= ViewPan[1];

	for (int i = 0; i < InputPointNumber - 1; i++)
	{
		seg_dir[0] = InputPoints[i + 1][0] - InputPoints[i][0];
		seg_dir[1] = InputPoints[i + 1][1] - InputPoints[i][1];

		len = sqrt(Dot2D(seg_dir, seg_dir));

		if (len < 5.0)
			continue;

		seg_dir[0] /= len;
		seg_dir[1] /= len;

		delta[0] = x - InputPoints[i][0];
		delta[1] = y - InputPoints[i][1];

		PerpVectLeft(seg_dir, perp);

		if (abs(Dot2D(delta, perp)) < 8.0
			&& Dot2D(delta, seg_dir) > 5.0 && Dot2D(delta, seg_dir) < len - 5.0)
		{
			SelectedSegment = i;
			break;
		}
	}

}
void MoveSelectedPt(int dx, int dy)
{
	int sel_vert = IsLineSelMode ? SelectedSegment : SelectedVertex;

	if (sel_vert == -1)
		return;

	if (SnapToGrid)
	{
		dx = floor((float)(MouseX - MouseClickX) / GridSpacing + 0.5) * GridSpacing;
		dy = floor((float)(MouseY - MouseClickY) / GridSpacing + 0.5) * GridSpacing;

		dx -= InputPoints[sel_vert][0] -
			floor(InputPoints[sel_vert][0] / GridSpacing) * GridSpacing;

		dy -= InputPoints[sel_vert][1] -
			floor(InputPoints[sel_vert][1] / GridSpacing) * GridSpacing;

		MouseClickX += dx;
		MouseClickY += dy;
	}

	InputPoints[sel_vert][0] += dx;
	InputPoints[sel_vert][1] += dy;

	if (IsLineSelMode)
	{
		InputPoints[sel_vert + 1][0] += dx;
		InputPoints[sel_vert + 1][1] += dy;
	}
}

void InsertPoint(int x, int y)
{
	if (SelectedSegment == -1)
		return;

	for (int i = InputPointNumber; i > SelectedSegment + 1; i--)
	{
		InputPoints[i][0] = InputPoints[i - 1][0];
		InputPoints[i][1] = InputPoints[i - 1][1];
	}
	InputPoints[SelectedSegment + 1][0] = x;
	InputPoints[SelectedSegment + 1][1] = y;

	InputPointNumber++;

	SelectedSegment = -1;
}
/***************************/
void Resize()
{
	glViewport(0, 0, SCRN_WDTH, SCRN_HGHT);

	glMatrixMode(GL_PROJECTION);
	glLoadIdentity();

	glOrtho(0, SCRN_WDTH, 0, SCRN_HGHT, -100, 100);

	glMatrixMode(GL_MODELVIEW);
	glLoadIdentity();
}

void Init()
{
	glShadeModel(GL_SMOOTH);
	glDisable(GL_TEXTURE_2D);
	glDisable(GL_LIGHTING);
	glDisable(GL_BLEND);

	glClearColor(0.0f, 0.0f, 0.0f, 1.0f);

	init_Fonts();
}

void CenterView()
{
	int min[2], max[2];

	ViewPan[0] = 0;
	ViewPan[1] = 0;

	if (InputPointNumber < 1)
		return;

	min[0] = max[0] = InputPoints[0][0];
	min[1] = max[1] = InputPoints[0][1];

	for (int i = 1; i < InputPointNumber; i++)
	{
		if (min[0] > InputPoints[i][0])
			min[0] = InputPoints[i][0];

		if (max[0] < InputPoints[i][0])
			max[0] = InputPoints[i][0];

		if (min[1] > InputPoints[i][1])
			min[1] = InputPoints[i][1];

		if (max[1] < InputPoints[i][1])
			max[1] = InputPoints[i][1];
	}

	ViewPan[0] = SCRN_WDTH / 2 - (min[0] + max[0]) * 0.5;
	ViewPan[1] = SCRN_HGHT / 2 - (min[1] + max[1]) * 0.5;
}
void PrintHelpString()
{
	char* str = "Controls:\nLeft drag: move control point\n"
		"Space+left drag: move control segment\n"
		"Right click: add control point\n"
		"Space+right click: insert control point on segment\n"
		"Control+right click: remove control point\n"
		"Middle button drag: pan view\n"
		"Control+middle button click: center view\n\n"
		"R: reset\n"
		"+: increase outline distance\n"
		"-: decrease outline distance\n"
		"S: snap to grid: on/off\n\n"
		"F1: show/hide help\n"
		"L: show/hide construction lines (red)\n"
		"C: show/hide contour vertices\n"
		"V: show/hide all vertices";

	if (ShowHelp)
		glColorPrint(10, SCRN_HGHT - 30, 500, NULL, "%s", str);
	else
		glColorPrint(10, SCRN_HGHT - 30, 500, NULL, "F1: help");
}

void RenderGrid()
{
	int start_x, end_x;
	int start_y, end_y;

	start_x = ViewPan[0];// + SCRN_WDTH;
	start_x /= GridSpacing;
	start_x *= -1;

	end_x = SCRN_WDTH - ViewPan[0];
	end_x /= GridSpacing;

	start_y = ViewPan[1];
	start_y /= GridSpacing;
	start_y *= -1;

	end_y = SCRN_HGHT - ViewPan[1];
	end_y /= GridSpacing;

	glBegin(GL_LINES);
	glColor4f(0, 0, 0.3, 1);

	for (int i = start_x; i < end_x + 1; i++)
	{
		if (abs(i) % 5)
			glColor4f(0, 0, 0.25, 1);
		else
			glColor4f(0, 0, 0.35, 1);

		glVertex2f(i * GridSpacing, 0 - ViewPan[1]);
		glVertex2f(i * GridSpacing, SCRN_HGHT - ViewPan[1]);
	}

	for (int i = start_y; i < end_y + 1; i++)
	{
		if (abs(i) % 5)
			glColor4f(0, 0, 0.25, 1);
		else
			glColor4f(0, 0, 0.35, 1);

		glVertex2f(0 - ViewPan[0], i * GridSpacing);
		glVertex2f(SCRN_WDTH - ViewPan[0], i * GridSpacing);
	}

	glEnd();
}
void Display()
{
	float colorTable[20][3] = {
		0.0,		0.9,	0.8,
								0.8,		0.3,	0.5,
								0.6,		0.6,	0.1,
								0.2,		0.3,	0.3,
								0.8,		0.0,	0.5,

								0.7,		1.0,	0.7,
								0.2,		0.4,	0.9,
								1.0,		0.7,	0.5,
								0.0,		0.4,	1.0,
								0.3,		0.0,	0.8
	};

	float t[] = { 0.2, 0.3, 1.0, 0.8, 0.4, 0.7, 0.9 };
	int indx[] = { 0, 1, 3, 2, 4, 5, 6 };


	glClear(GL_COLOR_BUFFER_BIT);

	glLoadIdentity();
	glTranslatef(ViewPan[0], ViewPan[1], 0);

	ValidateControlLine();

	CalculateRightLines();
	CalculateLeftLines();
	UpdateSideLineBuf();
	CalculateInterPoints();
	ValidateInterPoints();
	//MarchingEdges();

	ValidateSegments();

	glPointSize(5);
	//glLineWidth(3);	

	RenderGrid();

	glBegin(GL_LINES);
	// control line
	glColor4f(0.4, 0.4, 0.4, 1);
	for (int i = 0; i < InputPointNumber - 1; i++)
	{
		if (SelectedSegment == i)
			glColor4f(1, 0.5, 0, 1);
		else
			glColor4f(0.5, 0.5, 0.5, 1);

		glVertex2fv(InputPoints[i]);
		glVertex2fv(InputPoints[i + 1]);
	}
	// side lines
	if (AllLinesShown)
	{
		glColor4f(0.7, 0, 0, 1);
		for (int i = 0; i < SideLineNum; i++)
		{
			glVertex2fv(SideLines[i].vertex_start);
			glVertex2fv(SideLines[i].vertex_end);
		}
	}

	glEnd();

	glBegin(GL_POINTS);
	// control points

	for (int i = 0; i < InputPointNumber; i++)
	{
		if (SelectedVertex == i)
		{
			if (CtrPressed)
				glColor4f(0.7, 0, 0.4, 1);
			else
				glColor4f(1, 0.5, 0, 1);
		}
		else
			//glColor4f(1,1,1,1);
			glColor4f(0.5, 0.5, 0.5, 1);
		glVertex2fv(InputPoints[i]);
	}

	// all intersections
	if (AllVertShown)
	{
		glColor4f(0, 0, 1, 1);
		for (int i = 0; i < OutlineInterNum; i++)
		{
			glVertex2fv(OutlineInterPoints[i].vertex);
		}
	}
	//valid contour points
	if (ContourVertShown)
	{
		glColor4f(1, 1, 0, 1);
		for (int i = 0; i < OutlineInterNum; i++)
		{
			if (OutlineInterPoints[i].valid)
				glVertex2fv(OutlineInterPoints[i].vertex);
		}
	}


	glEnd();

	glBegin(GL_LINES);

	glColor4f(0, 1, 0, 1);
	for (int i = 0; i < OutlineValidSegmentNum; i++)
	{
		glVertex2fv(OutlineInterPoints[OutlineValidSegmentEnds[i][0]].vertex);
		glVertex2fv(OutlineInterPoints[OutlineValidSegmentEnds[i][1]].vertex);
	}


	glEnd();

	glLoadIdentity();

	glColor4f(1, 1, 1, 1);

	PrintHelpString();

	if (SnapToGrid)
		glPrint(10, 30, "Snap to grid: on");
	else
		glPrint(10, 30, "Snap to grid: off");

	SwapBuffers(win_state.hDC);
}

void CreateGlWindow()
{
	GLuint PixelFormat;
	WNDCLASS wc;

	WindowRect.left = (long)50;
	WindowRect.right = 50 + (long)SCRN_WDTH;
	WindowRect.top = (long)50;
	WindowRect.bottom = 50 + (long)SCRN_HGHT;


	win_state.hInstance = GetModuleHandle(NULL);

	wc.style = CS_HREDRAW | CS_VREDRAW | CS_OWNDC;
	wc.lpfnWndProc = (WNDPROC)WndProc;
	wc.cbClsExtra = 0;
	wc.cbWndExtra = 0;
	wc.hInstance = win_state.hInstance;
	wc.hIcon = LoadIcon(NULL, IDI_WINLOGO);
	wc.hCursor = LoadCursor(NULL, IDC_ARROW);
	wc.hbrBackground = NULL;
	wc.lpszMenuName = NULL;
	wc.lpszClassName = WIND_CLASS_NAME;

	RegisterClass(&wc);

	AdjustWindowRectEx(&WindowRect, WS_OVERLAPPEDWINDOW,
		FALSE, WS_EX_APPWINDOW | WS_EX_WINDOWEDGE);

	win_state.hWnd = CreateWindowEx(
		WS_EX_APPWINDOW | WS_EX_WINDOWEDGE,
		WIND_CLASS_NAME,
		WIND_CLASS_NAME,
		WS_OVERLAPPEDWINDOW | WS_CLIPCHILDREN | WS_POPUP,
		//WS_POPUP | WS_CLIPCHILDREN | WS_CLIPSIBLINGS,
		WindowRect.left, WindowRect.top,
		WindowRect.right - WindowRect.left,
		WindowRect.bottom - WindowRect.top,
		NULL,
		NULL,
		win_state.hInstance,
		NULL);

	static PIXELFORMATDESCRIPTOR pfd =
	{
		sizeof(PIXELFORMATDESCRIPTOR),
		1,
		PFD_DRAW_TO_WINDOW |
		PFD_SUPPORT_OPENGL |
		PFD_DOUBLEBUFFER,
		PFD_TYPE_RGBA,
		32,//32,					
		0, 0, 0, 0, 0, 0,
		0,
		0,
		0,
		0, 0, 0, 0,
		16,
		0,
		0,
		PFD_MAIN_PLANE,
		0,
		0, 0, 0
	};

	win_state.hDC = GetDC(win_state.hWnd);



	PixelFormat = ChoosePixelFormat(win_state.hDC, &pfd);
	SetPixelFormat(win_state.hDC, PixelFormat, &pfd);
	win_state.hRC = wglCreateContext(win_state.hDC);
	wglMakeCurrent(win_state.hDC, win_state.hRC);

	ShowWindow(win_state.hWnd, SW_SHOW);                        // Show The Window
	SetForegroundWindow(win_state.hWnd);                        // Slightly Higher Priority
	SetFocus(win_state.hWnd);                               // Sets Keyboard Focus To The Window
}

void KillGlWindow()
{
	wglMakeCurrent(NULL, NULL);
	wglDeleteContext(win_state.hRC);

	ReleaseDC(win_state.hWnd, win_state.hDC);

	DestroyWindow(win_state.hWnd);

	UnregisterClass(WIND_CLASS_NAME, win_state.hInstance);
}

#define __MOUSE_ROLL	0x020A
LRESULT CALLBACK WndProc(HWND hWnd, UINT uMsg, WPARAM wParam, LPARAM lParam)
{
	switch (uMsg)
	{
		case WM_CLOSE:

			PostQuitMessage(0);

			return 0;

		case WM_SIZE:

			SCRN_WDTH = LOWORD(lParam);
			SCRN_HGHT = HIWORD(lParam);

			Resize();

			//InvalidateRect(win_state.hWnd, NULL, FALSE);

			return 0;

		case WM_KEYDOWN:

			KeyboardDownHandler(wParam);

			if (!Lft_Btn)
			{
				if (IsLineSelMode)
					SelectSegment(MouseX, MouseY);
				else
					SelectPoint(MouseX, MouseY);
			}

			InvalidateRect(win_state.hWnd, NULL, FALSE);

			return 0;

		case WM_KEYUP:

			KeyboardUpHandler(wParam);

			if (!Lft_Btn)
			{
				if (IsLineSelMode)
					SelectSegment(MouseX, MouseY);
				else
					SelectPoint(MouseX, MouseY);
			}

			InvalidateRect(win_state.hWnd, NULL, FALSE);

			return 0;

		case WM_MOUSEMOVE:
			{
				int MouseDX;
				int MouseDY;

				MouseX = (int)(short)LOWORD(lParam);
				MouseY = SCRN_HGHT - (int)(short)HIWORD(lParam);

				MouseDX = MouseX - MousePrevX;
				MouseDY = MouseY - MousePrevY;

				MousePrevX = MouseX;
				MousePrevY = MouseY;

				if (Mdl_Btn)
				{
					ViewPan[0] += MouseDX;
					ViewPan[1] += MouseDY;
				}
				else if (Lft_Btn)
				{
					MoveSelectedPt(MouseDX, MouseDY);
				}
				else
				{
					if (IsLineSelMode)
					{
						SelectSegment(MouseX, MouseY);
					}
					else
					{
						SelectPoint(MouseX, MouseY);
					}
				}

				InvalidateRect(win_state.hWnd, NULL, FALSE);

				return 0;
			}

		case WM_LBUTTONDOWN:
			Lft_Btn = true;
			MouseClickX = MouseX;
			MouseClickY = MouseY;

			InvalidateRect(win_state.hWnd, NULL, FALSE);
			return 0;
		case WM_LBUTTONUP:
			Lft_Btn = false;
			InvalidateRect(win_state.hWnd, NULL, FALSE);
			return 0;

		case WM_MBUTTONDOWN:
			Mdl_Btn = true;
			SetCapture(win_state.hWnd);

			if (CtrPressed)
				CenterView();

			InvalidateRect(win_state.hWnd, NULL, FALSE);
			return 0;
		case WM_MBUTTONUP:
			Mdl_Btn = false;
			ReleaseCapture();
			InvalidateRect(win_state.hWnd, NULL, FALSE);
			return 0;

		case WM_RBUTTONDOWN:

			if (CtrPressed)
			{
				if (InputPointNumber >= 2 && SelectedVertex != -1)
				{
					for (int i = SelectedVertex; i < InputPointNumber - 1; i++)
					{
						InputPoints[i][0] = InputPoints[i + 1][0];
						InputPoints[i][1] = InputPoints[i + 1][1];
					}
					InputPointNumber--;
					SelectedVertex = -1;
				}
			}
			else if (InputPointNumber < MAX_POINTS)
			{
				int x = MouseX - ViewPan[0];
				int y = MouseY - ViewPan[1];

				if (IsLineSelMode)
				{
					InsertPoint(x, y);
				}
				else
				{
					if (SnapToGrid)
						Snap(x, y, &x, &y);

					InputPoints[InputPointNumber][0] = x;
					InputPoints[InputPointNumber][1] = y;

					InputPointNumber++;
				}
			}
			InvalidateRect(win_state.hWnd, NULL, FALSE);

			return 0;

		case WM_PAINT:

			Display();
			ValidateRect(win_state.hWnd, NULL);

			return 0;

		default:
			return DefWindowProc(hWnd, uMsg, wParam, lParam);
	}

	return DefWindowProc(hWnd, uMsg, wParam, lParam);
}

/**************************/

int WINAPI WinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, LPSTR lpCmdLine, int nCmdShow)
{
	MSG msg;

	CreateGlWindow();
	Init();
	Resize();
	///////////////

	Display();

	while (GetMessage(&msg, NULL, 0, 0))
	{
		if (msg.message == WM_QUIT)
		{
			break;
		}
		else
		{
			TranslateMessage(&msg);
			DispatchMessage(&msg);
		}
	}

	Kill_Font();
	KillGlWindow();

	return (msg.wParam);
}
