using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

namespace MaximovInk.AI
{



    public class FieldOfView : MonoBehaviour
    {
        public float viewRadius;
        [Range(0, 360)] public float viewAngle;

        public LayerMask targetMask;
        public LayerMask obstacleMask;

        public List<Transform> visibleTargets = new List<Transform>();

        public FieldOfView child;
        [Header("Mesh")] public float meshResolution;
        public int edgeResolveIterations;
        public MeshFilter viewMeshFilter;
        public float edgeDstThresoult;
        private Mesh viewMesh;

        private void Start()
        {
            viewMesh = new Mesh();
            viewMesh.name = "ViewMesh";
            viewMeshFilter.mesh = viewMesh;
            StartCoroutine("FindTargetsWithDeley", .2f);
        }

        IEnumerator FindTargetsWithDeley(float delay)
        {
            while (true)
            {
                yield return new WaitForSeconds(delay);
                FindVisibleTargets();
            }
        }

        public void SetData(FowData data)
        {
            viewRadius = data.Radius;
            viewAngle = data.Angle;
        }

        void FindVisibleTargets()
        {
            Collider2D[] targets = Physics2D.OverlapCircleAll(transform.position, viewRadius, targetMask);

            
            visibleTargets.Clear();

            for (int i = 0; i < targets.Length; i++)
            {
                Transform target = targets[i].transform;
                Vector2 dirToTarget = (target.position - transform.position).normalized;

                if (Vector2.Angle(transform.right, dirToTarget) < viewAngle / 2)
                {
                    float distToTarget = Vector3.Distance(transform.position, target.position);
                    if (!Physics2D.Raycast(transform.position, dirToTarget, distToTarget, obstacleMask))
                    {
                        visibleTargets.Add(target);
                    }
                }
            }
        }

        public List<FieldOfView> GetChildsFow()
        {
            List<FieldOfView> fows = new List<FieldOfView>();
            if (child != null)
            {
                fows.Add(child);
                fows.AddRange(child.GetChildsFow());
            }

            return fows;
        }

        public List<Transform> GetAllVisibleChilds()
        {
            List<Transform> end = new List<Transform>();
            end.AddRange(visibleTargets);
            if (child != null)
            {
                end.AddRange(child.GetAllVisibleChilds());
            }

            return end;
        }

        public Vector3 DirFromAngle(float angleInDegrees, bool global)
        {
            if (!global)
                angleInDegrees += +90 - transform.eulerAngles.z;

            return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), Mathf.Cos(angleInDegrees * Mathf.Deg2Rad),
                0);
        }

        private void LateUpdate()
        {
            DrawFieldOfView();
        }

        ViewCastInfo ViewCast(float globalAngle)
        {
            Vector3 dir = DirFromAngle(globalAngle, true);
            RaycastHit2D hit;
            if (hit = Physics2D.Raycast(transform.position, dir, viewRadius, obstacleMask))
            {
                return new ViewCastInfo(true, hit.point, hit.distance, globalAngle);
            }

            return new ViewCastInfo(false, transform.position + dir * viewRadius, viewRadius, globalAngle);
        }

        EdgeInfo FindEdge(ViewCastInfo minViewCast, ViewCastInfo maxViewCast)
        {
            float minAngle = minViewCast.angle;
            float maxAngle = maxViewCast.angle;
            Vector2 minPoint = Vector2.zero;
            Vector2 maxPoint = Vector2.zero;

            for (int i = 0; i < edgeResolveIterations; i++)
            {
                float angle = (minAngle + maxAngle) / 2;
                ViewCastInfo newViewCast = ViewCast(angle);

                bool edgeDstThresoultExceeded = Mathf.Abs(minViewCast.dst - newViewCast.dst) > edgeDstThresoult;

                if (newViewCast.hit == minViewCast.hit && !edgeDstThresoultExceeded)
                {
                    minAngle = angle;
                    minPoint = newViewCast.point;
                }
                else
                {
                    maxAngle = angle;
                    maxPoint = newViewCast.point;
                }
            }

            return new EdgeInfo(minPoint, maxPoint);
        }

        public void DrawFieldOfView()
        {
            int stepCount = Mathf.RoundToInt(viewAngle * meshResolution);
            float stepAngleSize = viewAngle / stepCount;
            List<Vector3> viewPoints = new List<Vector3>();
            ViewCastInfo oldViewCast = new ViewCastInfo();
            for (int i = 0; i <= stepCount; i++)
            {
                float angle = (-transform.eulerAngles.z - viewAngle / 2 + stepAngleSize * i) + 90;
                // Debug.DrawLine(transform.position, transform.position + DirFromAngle(angle, true) * viewRadius,Color.red);
                ViewCastInfo newViewCast = ViewCast(angle);

                if (i > 0)
                {
                    bool edgeDstThresoultExceeded = Mathf.Abs(oldViewCast.dst - newViewCast.dst) > edgeDstThresoult;


                    if (oldViewCast.hit != newViewCast.hit ||
                        (oldViewCast.hit && newViewCast.hit && edgeDstThresoultExceeded))
                    {
                        EdgeInfo edge = FindEdge(oldViewCast, newViewCast);

                        if (edge.pointA != Vector2.zero)
                        {
                            viewPoints.Add(edge.pointA);
                        }

                        if (edge.pointB != Vector2.zero)
                        {
                            viewPoints.Add(edge.pointB);
                        }
                    }
                }

                viewPoints.Add(newViewCast.point);
                oldViewCast = newViewCast;
            }

            int vertexCount = viewPoints.Count + 1;

            Vector3[] vertices = new Vector3[vertexCount];
            int[] triangles = new int[(vertexCount - 2) * 3];

            vertices[0] = Vector3.zero;

            for (int i = 0; i < vertexCount - 1; i++)
            {
                vertices[i + 1] = transform.InverseTransformPoint(viewPoints[i]);

                if (i < vertexCount - 2)
                {
                    triangles[i * 3] = 0;
                    triangles[i * 3 + 1] = i + 1;
                    triangles[i * 3 + 2] = i + 2;
                }
            }

            viewMesh.Clear();
            viewMesh.vertices = vertices;
            viewMesh.triangles = triangles;

            viewMesh.RecalculateNormals();

        }
    }

    public struct ViewCastInfo
    {
        public bool hit;
        public Vector2 point;
        public float dst;
        public float angle;

        public ViewCastInfo(bool _hit, Vector2 _point, float _dst, float _angle)
        {
            hit = _hit;
            point = _point;
            dst = _dst;
            angle = _angle;
        }
    }

    public struct EdgeInfo
    {
        public Vector2 pointA, pointB;

        public EdgeInfo(Vector2 _pointA, Vector2 _pointB)
        {
            pointA = _pointA;
            pointB = _pointB;
        }
    }

    [Serializable]
    public class FowData
    {
        public float Radius;
        public float Angle;
    }

}