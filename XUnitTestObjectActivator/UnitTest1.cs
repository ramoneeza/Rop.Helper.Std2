using System;
using System.Net.Http.Headers;
using Xunit;
using Rop.ObjectActivator;
namespace XUnitTestObjectActivator
{
    public class UnitTest1
    {
        public class Point:IEquatable<Point>
        {
            public int X { get; set; }
            public int Y { get; set; }

            public Point(int x, int y)
            {
                X = x;
                Y = y;
            }

            public bool Equals(Point other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return X == other.X && Y == other.Y;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((Point) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (X * 397) ^ Y;
                }
            }
        }
        [Fact]
        public void Test1()
        {
            var p1 = new Point(10, 20);
            var p2 = ObjectActivatorFactory.FactoryExact<Point>(10, 20);
            Assert.Equal(p1,p2);
        }
    }
}
