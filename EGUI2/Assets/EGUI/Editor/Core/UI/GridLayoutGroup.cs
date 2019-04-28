using UnityEngine;

namespace EGUI.UI
{
    [Persistence]
    public class GridLayoutGroup : LayoutGroup
    {
        public enum Corner
        {
            UpperLeft,
            UpperRight,
            LowerLeft,
            LowerRight,
        }

        public enum Axis
        {
            Horizontal,
            Vertical,
        }

        public enum Constraint
        {
            Flexible,
            FixedRowCount,
            FixedColumnCount,
        }

        [PersistentField]
        private Corner mStartCorner = Corner.UpperLeft;

        public Corner startCorner { get { return mStartCorner; } set { mStartCorner = value; } }

        [PersistentField]
        private Axis mStartAxis = Axis.Horizontal;

        public Axis startAxis { get { return mStartAxis; } set { mStartAxis = value; } }

        [PersistentField]
        private Vector2 mCellSize = new Vector2(100, 100);

        public Vector2 cellSize { get { return mCellSize; } set { mCellSize = value; } }

        [PersistentField]
        private Constraint mConstraint = Constraint.Flexible;

        public Constraint constraint { get { return mConstraint; } set { mConstraint = value; } }

        [PersistentField]
        private int mConstraintCount = 1;

        public int constraintCount { get { return mConstraintCount; } set { mConstraintCount = value; } }

        protected override Vector2 GetLayoutSize()
        {
            int row, column;
            CalcRowColumnCount(out row, out column);
            return new Vector2(
                cellSize.x * column + spacing.x * (column - 1),
                cellSize.y * row + spacing.y * (row - 1)
            );
        }

        protected override Vector2 GetChildOffset(Node child)
        {
            var offset = base.GetChildOffset(child);
            if (childAlignment == Alignment.UpperCenter ||
                childAlignment == Alignment.MiddleCenter ||
                childAlignment == Alignment.LowerCenter)
            {
                offset.x -= layoutSize.x * 0.5f;
            }
            if (childAlignment == Alignment.UpperRight ||
                childAlignment == Alignment.MiddleRight ||
                childAlignment == Alignment.LowerRight)
            {
                offset.x -= layoutSize.x;
            }
            if (childAlignment == Alignment.MiddleLeft ||
                childAlignment == Alignment.MiddleCenter ||
                childAlignment == Alignment.MiddleRight)
            {
                offset.y -= layoutSize.y * 0.5f;
            }
            if (childAlignment == Alignment.LowerLeft ||
                childAlignment == Alignment.LowerCenter ||
                childAlignment == Alignment.LowerRight)
            {
                offset.y -= layoutSize.y;
            }
            return offset;
        }

        public override void Update()
        {
            base.Update();
            int rowCount, columnCount;
            CalcRowColumnCount(out rowCount, out columnCount);
            Vector2 start, step;
            CalcLayoutParams(rowCount, columnCount, out start, out step);
            var offset = start;
            var childCount = layoutChildren.Count;
            if (startAxis == Axis.Horizontal)
            {
                for (int i = 0; i < childCount; i++)
                {
                    var child = layoutChildren[i];
                    child.localPosition = offset + GetChildOffset(child);
                    child.size = cellSize;
                    var row = i / columnCount;
                    var column = i % columnCount;
                    if (column == columnCount - 1 ||
                        (constraint == Constraint.FixedRowCount && childCount - i <= rowCount - row))
                    {
                        offset.x = start.x;
                        offset.y += step.y;
                    }
                    else
                    {
                        offset.x += step.x;
                    }
                }
            }
            else
            {
                for (int i = 0; i < childCount; i++)
                {
                    var child = layoutChildren[i];
                    child.localPosition = offset + GetChildOffset(child);
                    child.size = cellSize;
                    var row = i % rowCount;
                    var column = i / rowCount;
                    if (row == rowCount - 1 ||
                        (constraint == Constraint.FixedColumnCount && childCount - i <= columnCount - column))
                    {
                        offset.y = start.y;
                        offset.x += step.x;
                    }
                    else
                    {
                        offset.y += step.y;
                    }
                }
            }
        }

        public override Vector2 GetContentSize()
        {
            return layoutSize;
        }

        private void CalcRowColumnCount(out int rowCount, out int columnCount)
        {
            var childCount = layoutChildren.Count;
            switch (constraint)
            {
                case Constraint.Flexible:
                    {
                        if (startAxis == Axis.Horizontal)
                        {
                            columnCount = Mathf.Clamp(Mathf.FloorToInt(node.size.x / cellSize.x), 1, childCount);
                            rowCount = Mathf.CeilToInt((float)childCount / columnCount);
                        }
                        else
                        {
                            rowCount = Mathf.Clamp(Mathf.FloorToInt(node.size.y / cellSize.y), 1, childCount);
                            columnCount = Mathf.CeilToInt((float)childCount / rowCount);
                        }
                        break;
                    }
                case Constraint.FixedColumnCount:
                    {
                        columnCount = Mathf.Clamp(constraintCount, 1, childCount);
                        rowCount = Mathf.CeilToInt((float)childCount / columnCount);
                        break;
                    }
                case Constraint.FixedRowCount:
                    {
                        rowCount = Mathf.Clamp(constraintCount, 1, childCount);
                        columnCount = Mathf.CeilToInt((float)childCount / constraintCount);
                        break;
                    }
                default:
                    throw new System.NotSupportedException();
            }
        }

        private void CalcLayoutParams(int rowCount, int columnCount, out Vector2 start, out Vector2 step)
        {
            step = cellSize + spacing;
            switch (startCorner)
            {
                case Corner.UpperLeft:
                    {
                        start = Vector2.zero;
                        break;
                    }
                case Corner.UpperRight:
                    {
                        start = new Vector2((columnCount - 1) * step.x, 0);
                        step.x = -step.x;
                        break;
                    }
                case Corner.LowerLeft:
                    {
                        start = new Vector2(0, (rowCount - 1) * step.y);
                        step.y = -step.y;
                        break;
                    }
                case Corner.LowerRight:
                    {
                        start = new Vector2((columnCount - 1) * step.x, (rowCount - 1) * step.y);
                        step = -step;
                        break;
                    }
                default:
                    {
                        throw new System.NotSupportedException();
                    }
            }
        }
    }
}
