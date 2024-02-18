using SatisfactorySaveNet.Abstracts.Maths.Matrix;
using SatisfactorySaveNet.Abstracts.Maths.Vector;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using System.Xml.Serialization;

namespace SatisfactorySaveNet.Abstracts.Maths.Data
{
    /// <summary>
    /// Represents a double-precision Quaternion.
    /// </summary>
    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct QuaternionD : IEquatable<QuaternionD>, IFormattable
    {
        /// <summary>
        /// The X, Y and Z components of this instance.
        /// </summary>
        public Vector3D Xyz;

        /// <summary>
        /// The W component of this instance.
        /// </summary>
        public double W;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuaternionD"/> struct.
        /// </summary>
        /// <param name="v">The vector part.</param>
        /// <param name="w">The w part.</param>
        public QuaternionD(Vector3D v, double w)
        {
            Xyz = v;
            W = w;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QuaternionD"/> struct.
        /// </summary>
        /// <param name="x">The x component.</param>
        /// <param name="y">The y component.</param>
        /// <param name="z">The z component.</param>
        /// <param name="w">The w component.</param>
        public QuaternionD(double x, double y, double z, double w)
            : this(new Vector3D(x, y, z), w)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QuaternionD"/> struct from given Euler angles in radians.
        /// </summary>
        /// <param name="pitch">The pitch (attitude), rotation around X axis.</param>
        /// <param name="yaw">The yaw (heading), rotation around Y axis.</param>
        /// <param name="roll">The roll (bank), rotation around Z axis.</param>
        public QuaternionD(double pitch, double yaw, double roll)
        {
            yaw *= 0.5;
            pitch *= 0.5;
            roll *= 0.5;

            double c1 = Math.Cos(yaw);
            double c2 = Math.Cos(pitch);
            double c3 = Math.Cos(roll);
            double s1 = Math.Sin(yaw);
            double s2 = Math.Sin(pitch);
            double s3 = Math.Sin(roll);

            W = (c1 * c2 * c3) - (s1 * s2 * s3);
            Xyz.X = (s1 * c2 * c3) + (c1 * s2 * s3);
            Xyz.Y = (c1 * s2 * c3) - (s1 * c2 * s3);
            Xyz.Z = (c1 * c2 * s3) + (s1 * s2 * c3);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QuaternionD"/> struct from given Euler angles in radians.
        /// </summary>
        /// <param name="eulerAngles">The euler angles as a Vector3d.</param>
        public QuaternionD(Vector3D eulerAngles)
            : this(eulerAngles.X, eulerAngles.Y, eulerAngles.Z)
        {
        }

        /// <summary>
        /// Gets or sets the X component of this instance.
        /// </summary>
        [XmlIgnore]
        public double X
        {
            readonly get => Xyz.X;
            set => Xyz.X = value;
        }

        /// <summary>
        /// Gets or sets the Y component of this instance.
        /// </summary>
        [XmlIgnore]
        public double Y
        {
            readonly get => Xyz.Y;
            set => Xyz.Y = value;
        }

        /// <summary>
        /// Gets or sets the Z component of this instance.
        /// </summary>
        [XmlIgnore]
        public double Z
        {
            readonly get => Xyz.Z;
            set => Xyz.Z = value;
        }

        /// <summary>
        /// Convert the current quaternion to axis angle representation.
        /// </summary>
        /// <param name="axis">The resultant axis.</param>
        /// <param name="angle">The resultant angle.</param>
        public void ToAxisAngle(out Vector3D axis, out double angle)
        {
            Vector4D result = ToAxisAngle();
            axis = result.Xyz;
            angle = result.W;
        }

        /// <summary>
        /// Convert this instance to an axis-angle representation.
        /// </summary>
        /// <returns>A Vector4 that is the axis-angle representation of this quaternion.</returns>
        public Vector4D ToAxisAngle()
        {
            QuaternionD q = this;
            if (Math.Abs(q.W) > 1.0d)
            {
                q.Normalize();
            }

            Vector4D result = new()
            {
                W = 2.0d * Math.Acos(q.W) // angle
            };

            double den = Math.Sqrt(1.0 - (q.W * q.W));
            if (den > 0.0001d)
            {
                result.Xyz = q.Xyz / den;
            }
            else
            {
                // This occurs when the angle is zero.
                // Not a problem: just set an arbitrary normalized axis.
                result.Xyz = Vector3D.UnitX;
            }

            return result;
        }

        /// <summary>
        /// Convert the current quaternion to Euler angle representation.
        /// </summary>
        /// <param name="angles">The Euler angles in radians.</param>
        public readonly void ToEulerAngles(out Vector3D angles)
        {
            angles = ToEulerAngles();
        }

        /// <summary>
        /// Convert this instance to an Euler angle representation.
        /// </summary>
        /// <returns>The Euler angles in radians.</returns>
        public readonly Vector3D ToEulerAngles()
        {
            /*
            reference
            http://en.wikipedia.org/wiki/Conversion_between_quaternions_and_Euler_angles
            http://www.euclideanspace.com/maths/geometry/rotations/conversions/quaternionToEuler/
            */

            QuaternionD q = this;

            Vector3D eulerAngles;

            // Threshold for the singularities found at the north/south poles.
            const double SINGULARITY_THRESHOLD = 0.4999995;

            double sqw = q.W * q.W;
            double sqx = q.X * q.X;
            double sqy = q.Y * q.Y;
            double sqz = q.Z * q.Z;
            double unit = sqx + sqy + sqz + sqw; // if normalised is one, otherwise is correction factor
            double singularityTest = (q.X * q.Z) + (q.W * q.Y);

            if (singularityTest > SINGULARITY_THRESHOLD * unit)
            {
                eulerAngles.Z = 2 * Math.Atan2(q.X, q.W);
                eulerAngles.Y = MathHelper.PiOver2;
                eulerAngles.X = 0;
            }
            else if (singularityTest < -SINGULARITY_THRESHOLD * unit)
            {
                eulerAngles.Z = -2 * Math.Atan2(q.X, q.W);
                eulerAngles.Y = -MathHelper.PiOver2;
                eulerAngles.X = 0;
            }
            else
            {
                eulerAngles.Z = Math.Atan2(2 * ((q.W * q.Z) - (q.X * q.Y)), sqw + sqx - sqy - sqz);
                eulerAngles.Y = Math.Asin(2 * singularityTest / unit);
                eulerAngles.X = Math.Atan2(2 * ((q.W * q.X) - (q.Y * q.Z)), sqw - sqx - sqy + sqz);
            }

            return eulerAngles;
        }

        /// <summary>
        /// Gets the length (magnitude) of the Quaterniond.
        /// </summary>
        /// <seealso cref="LengthSquared"/>
        public readonly double Length => Math.Sqrt((W * W) + Xyz.LengthSquared);

        /// <summary>
        /// Gets the square of the Quaterniond length (magnitude).
        /// </summary>
        public readonly double LengthSquared => (W * W) + Xyz.LengthSquared;

        /// <summary>
        /// Returns a copy of the Quaterniond scaled to unit length.
        /// </summary>
        /// <returns>The normalized copy.</returns>
        public readonly QuaternionD Normalized()
        {
            QuaternionD q = this;
            q.Normalize();
            return q;
        }

        /// <summary>
        /// Inverts this Quaterniond.
        /// </summary>
        public void Invert() => Invert(in this, out this);

        /// <summary>
        /// Returns the inverse of this Quaterniond.
        /// </summary>
        /// <returns>The inverted copy.</returns>
        public readonly QuaternionD Inverted()
        {
            QuaternionD q = this;
            q.Invert();
            return q;
        }

        /// <summary>
        /// Scales the Quaterniond to unit length.
        /// </summary>
        public void Normalize()
        {
            double scale = 1.0d / Length;
            Xyz *= scale;
            W *= scale;
        }

        /// <summary>
        /// Inverts the Vector3d component of this Quaterniond.
        /// </summary>
        public void Conjugate()
        {
            Xyz = -Xyz;
        }

        /// <summary>
        /// Defines the identity quaternion.
        /// </summary>
        public static readonly QuaternionD Identity = new(0, 0, 0, 1);

        /// <summary>
        /// Add two quaternions.
        /// </summary>
        /// <param name="left">The first operand.</param>
        /// <param name="right">The second operand.</param>
        /// <returns>The result of the addition.</returns>
        [Pure]
        public static QuaternionD Add(QuaternionD left, QuaternionD right) => new(
                left.Xyz + right.Xyz,
                left.W + right.W);

        /// <summary>
        /// Add two quaternions.
        /// </summary>
        /// <param name="left">The first operand.</param>
        /// <param name="right">The second operand.</param>
        /// <param name="result">The result of the addition.</param>
        public static void Add(in QuaternionD left, in QuaternionD right, out QuaternionD result)
        {
            result = new QuaternionD(
                left.Xyz + right.Xyz,
                left.W + right.W);
        }

        /// <summary>
        /// Subtracts two instances.
        /// </summary>
        /// <param name="left">The left instance.</param>
        /// <param name="right">The right instance.</param>
        /// <returns>The result of the operation.</returns>
        [Pure]
        public static QuaternionD Sub(QuaternionD left, QuaternionD right) => new(
                left.Xyz - right.Xyz,
                left.W - right.W);

        /// <summary>
        /// Subtracts two instances.
        /// </summary>
        /// <param name="left">The left instance.</param>
        /// <param name="right">The right instance.</param>
        /// <param name="result">The result of the operation.</param>
        public static void Sub(in QuaternionD left, in QuaternionD right, out QuaternionD result)
        {
            result = new QuaternionD(
                left.Xyz - right.Xyz,
                left.W - right.W);
        }

        /// <summary>
        /// Multiplies two instances.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>A new instance containing the result of the calculation.</returns>
        [Pure]
        public static QuaternionD Multiply(QuaternionD left, QuaternionD right)
        {
            Multiply(in left, in right, out QuaternionD result);
            return result;
        }

        /// <summary>
        /// Multiplies two instances.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <param name="result">A new instance containing the result of the calculation.</param>
        public static void Multiply(in QuaternionD left, in QuaternionD right, out QuaternionD result)
        {
            result = new QuaternionD(
                (right.W * left.Xyz) + (left.W * right.Xyz) + Vector3D.Cross(left.Xyz, right.Xyz),
                (left.W * right.W) - Vector3D.Dot(left.Xyz, right.Xyz));
        }

        /// <summary>
        /// Multiplies an instance by a scalar.
        /// </summary>
        /// <param name="quaternion">The instance.</param>
        /// <param name="scale">The scalar.</param>
        /// <param name="result">A new instance containing the result of the calculation.</param>
        public static void Multiply(in QuaternionD quaternion, double scale, out QuaternionD result)
        {
            result = new QuaternionD
            (
                quaternion.X * scale,
                quaternion.Y * scale,
                quaternion.Z * scale,
                quaternion.W * scale
            );
        }

        /// <summary>
        /// Multiplies an instance by a scalar.
        /// </summary>
        /// <param name="quaternion">The instance.</param>
        /// <param name="scale">The scalar.</param>
        /// <returns>A new instance containing the result of the calculation.</returns>
        [Pure]
        public static QuaternionD Multiply(QuaternionD quaternion, double scale) => new(
                quaternion.X * scale,
                quaternion.Y * scale,
                quaternion.Z * scale,
                quaternion.W * scale
            );

        /// <summary>
        /// Get the conjugate of the given Quaterniond.
        /// </summary>
        /// <param name="q">The Quaterniond.</param>
        /// <returns>The conjugate of the given Quaterniond.</returns>
        [Pure]
        public static QuaternionD Conjugate(QuaternionD q) => new(-q.Xyz, q.W);

        /// <summary>
        /// Get the conjugate of the given Quaterniond.
        /// </summary>
        /// <param name="q">The Quaterniond.</param>
        /// <param name="result">The conjugate of the given Quaterniond.</param>
        public static void Conjugate(in QuaternionD q, out QuaternionD result)
        {
            result = new QuaternionD(-q.Xyz, q.W);
        }

        /// <summary>
        /// Get the inverse of the given Quaterniond.
        /// </summary>
        /// <param name="q">The Quaterniond to invert.</param>
        /// <returns>The inverse of the given Quaterniond.</returns>
        [Pure]
        public static QuaternionD Invert(QuaternionD q)
        {
            Invert(in q, out QuaternionD result);
            return result;
        }

        /// <summary>
        /// Get the inverse of the given Quaterniond.
        /// </summary>
        /// <param name="q">The Quaterniond to invert.</param>
        /// <param name="result">The inverse of the given Quaterniond.</param>
        public static void Invert(in QuaternionD q, out QuaternionD result)
        {
            double lengthSq = q.LengthSquared;
            if (lengthSq != 0.0)
            {
                double i = 1.0d / lengthSq;
                result = new QuaternionD(q.Xyz * -i, q.W * i);
            }
            else
            {
                result = q;
            }
        }

        /// <summary>
        /// Scale the given Quaterniond to unit length.
        /// </summary>
        /// <param name="q">The Quaterniond to normalize.</param>
        /// <returns>The normalized copy.</returns>
        [Pure]
        public static QuaternionD Normalize(QuaternionD q)
        {
            Normalize(in q, out QuaternionD result);
            return result;
        }

        /// <summary>
        /// Scale the given Quaterniond to unit length.
        /// </summary>
        /// <param name="q">The Quaterniond to normalize.</param>
        /// <param name="result">The normalized Quaterniond.</param>
        public static void Normalize(in QuaternionD q, out QuaternionD result)
        {
            double scale = 1.0d / q.Length;
            result = new QuaternionD(q.Xyz * scale, q.W * scale);
        }

        /// <summary>
        /// Build a Quaterniond from the given axis and angle.
        /// </summary>
        /// <param name="axis">The axis to rotate about.</param>
        /// <param name="angle">The rotation angle in radians.</param>
        /// <returns>The quaternion.</returns>
        [Pure]
        public static QuaternionD FromAxisAngle(Vector3D axis, double angle)
        {
            if (axis.LengthSquared == 0.0d)
            {
                return Identity;
            }

            QuaternionD result = Identity;

            angle *= 0.5d;
            axis.Normalize();
            result.Xyz = axis * Math.Sin(angle);
            result.W = Math.Cos(angle);

            return Normalize(result);
        }

        /// <summary>
        /// Builds a Quaterniond from the given euler angles.
        /// </summary>
        /// <param name="pitch">The pitch (attitude), rotation around X axis.</param>
        /// <param name="yaw">The yaw (heading), rotation around Y axis.</param>
        /// <param name="roll">The roll (bank), rotation around Z axis.</param>
        /// <returns>The quaternion.</returns>
        [Pure]
        public static QuaternionD FromEulerAngles(double pitch, double yaw, double roll) => new(pitch, yaw, roll);

        /// <summary>
        /// Builds a Quaterniond from the given euler angles.
        /// </summary>
        /// <param name="eulerAngles">The euler angles as a vector.</param>
        /// <returns>The equivalent Quaterniond.</returns>
        [Pure]
        public static QuaternionD FromEulerAngles(Vector3D eulerAngles) => new(eulerAngles);

        /// <summary>
        /// Builds a Quaterniond from the given euler angles.
        /// </summary>
        /// <param name="eulerAngles">The euler angles as a vector.</param>
        /// <param name="result">The equivalent Quaterniond.</param>
        public static void FromEulerAngles(in Vector3D eulerAngles, out QuaternionD result)
        {
            double c1 = Math.Cos(eulerAngles.Y * 0.5);
            double c2 = Math.Cos(eulerAngles.X * 0.5);
            double c3 = Math.Cos(eulerAngles.Z * 0.5);
            double s1 = Math.Sin(eulerAngles.Y * 0.5);
            double s2 = Math.Sin(eulerAngles.X * 0.5);
            double s3 = Math.Sin(eulerAngles.Z * 0.5);

            result.W = (c1 * c2 * c3) - (s1 * s2 * s3);
            result.Xyz.X = (s1 * s2 * c3) + (c1 * c2 * s3);
            result.Xyz.Y = (s1 * c2 * c3) + (c1 * s2 * s3);
            result.Xyz.Z = (c1 * s2 * c3) - (s1 * c2 * s3);
        }

        /// <summary>
        /// Converts a quaternion to it's euler angle representation.
        /// </summary>
        /// <param name="q">The Quaternion.</param>
        /// <param name="result">The resulting euler angles in radians.</param>
        public static void ToEulerAngles(in QuaternionD q, out Vector3D result) => q.ToEulerAngles(out result);

        /// <summary>
        /// Builds a quaternion from the given rotation matrix.
        /// </summary>
        /// <param name="matrix">A rotation matrix.</param>
        /// <returns>The equivalent quaternion.</returns>
        [Pure]
        public static QuaternionD FromMatrix(Matrix3D matrix)
        {
            FromMatrix(in matrix, out QuaternionD result);
            return result;
        }

        /// <summary>
        /// Builds a quaternion from the given rotation matrix.
        /// </summary>
        /// <param name="matrix">A rotation matrix.</param>
        /// <param name="result">The equivalent quaternion.</param>
        public static void FromMatrix(in Matrix3D matrix, out QuaternionD result)
        {
            double trace = matrix.Trace;

            if (trace > 0)
            {
                double s = Math.Sqrt(trace + 1) * 2;
                double invS = 1.0 / s;

                result.W = s * 0.25;
                result.Xyz.X = (matrix.Row2.Y - matrix.Row1.Z) * invS;
                result.Xyz.Y = (matrix.Row0.Z - matrix.Row2.X) * invS;
                result.Xyz.Z = (matrix.Row1.X - matrix.Row0.Y) * invS;
            }
            else
            {
                double m00 = matrix.Row0.X, m11 = matrix.Row1.Y, m22 = matrix.Row2.Z;

                if (m00 > m11 && m00 > m22)
                {
                    double s = Math.Sqrt(1 + m00 - m11 - m22) * 2;
                    double invS = 1.0 / s;

                    result.W = (matrix.Row2.Y - matrix.Row1.Z) * invS;
                    result.Xyz.X = s * 0.25;
                    result.Xyz.Y = (matrix.Row0.Y + matrix.Row1.X) * invS;
                    result.Xyz.Z = (matrix.Row0.Z + matrix.Row2.X) * invS;
                }
                else if (m11 > m22)
                {
                    double s = Math.Sqrt(1 + m11 - m00 - m22) * 2;
                    double invS = 1.0 / s;

                    result.W = (matrix.Row0.Z - matrix.Row2.X) * invS;
                    result.Xyz.X = (matrix.Row0.Y + matrix.Row1.X) * invS;
                    result.Xyz.Y = s * 0.25;
                    result.Xyz.Z = (matrix.Row1.Z + matrix.Row2.Y) * invS;
                }
                else
                {
                    double s = Math.Sqrt(1 + m22 - m00 - m11) * 2;
                    double invS = 1.0 / s;

                    result.W = (matrix.Row1.X - matrix.Row0.Y) * invS;
                    result.Xyz.X = (matrix.Row0.Z + matrix.Row2.X) * invS;
                    result.Xyz.Y = (matrix.Row1.Z + matrix.Row2.Y) * invS;
                    result.Xyz.Z = s * 0.25;
                }
            }
        }

        /// <summary>
        /// Do Spherical linear interpolation between two quaternions.
        /// </summary>
        /// <param name="q1">The first Quaterniond.</param>
        /// <param name="q2">The second Quaterniond.</param>
        /// <param name="blend">The blend factor.</param>
        /// <returns>A smooth blend between the given quaternions.</returns>
        [Pure]
        public static QuaternionD Slerp(QuaternionD q1, QuaternionD q2, double blend)
        {
            // if either input is zero, return the other.
            if (q1.LengthSquared == 0.0d)
            {
                return q2.LengthSquared == 0.0d ? Identity : q2;
            }

            if (q2.LengthSquared == 0.0d)
            {
                return q1;
            }

            double cosHalfAngle = (q1.W * q2.W) + Vector3D.Dot(q1.Xyz, q2.Xyz);

            if (cosHalfAngle is >= 1.0d or <= (-1.0d))
            {
                // angle = 0.0d, so just return one input.
                return q1;
            }

            if (cosHalfAngle < 0.0d)
            {
                q2.Xyz = -q2.Xyz;
                q2.W = -q2.W;
                cosHalfAngle = -cosHalfAngle;
            }

            double blendA;
            double blendB;
            if (cosHalfAngle < 0.99d)
            {
                // do proper slerp for big angles
                double halfAngle = Math.Acos(cosHalfAngle);
                double sinHalfAngle = Math.Sin(halfAngle);
                double oneOverSinHalfAngle = 1.0d / sinHalfAngle;
                blendA = Math.Sin(halfAngle * (1.0d - blend)) * oneOverSinHalfAngle;
                blendB = Math.Sin(halfAngle * blend) * oneOverSinHalfAngle;
            }
            else
            {
                // do lerp if angle is really small.
                blendA = 1.0d - blend;
                blendB = blend;
            }

            QuaternionD result = new((blendA * q1.Xyz) + (blendB * q2.Xyz), (blendA * q1.W) + (blendB * q2.W));
            return result.LengthSquared > 0.0d ? Normalize(result) : Identity;
        }

        /// <summary>
        /// Adds two instances.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>The result of the calculation.</returns>
        [Pure]
        public static QuaternionD operator +(QuaternionD left, QuaternionD right)
        {
            left.Xyz += right.Xyz;
            left.W += right.W;
            return left;
        }

        /// <summary>
        /// Subtracts two instances.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>The result of the calculation.</returns>
        [Pure]
        public static QuaternionD operator -(QuaternionD left, QuaternionD right)
        {
            left.Xyz -= right.Xyz;
            left.W -= right.W;
            return left;
        }

        /// <summary>
        /// Multiplies two instances.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>The result of the calculation.</returns>
        [Pure]
        public static QuaternionD operator *(QuaternionD left, QuaternionD right)
        {
            Multiply(in left, in right, out left);
            return left;
        }

        /// <summary>
        /// Multiplies an instance by a scalar.
        /// </summary>
        /// <param name="quaternion">The instance.</param>
        /// <param name="scale">The scalar.</param>
        /// <returns>A new instance containing the result of the calculation.</returns>
        [Pure]
        public static QuaternionD operator *(QuaternionD quaternion, double scale)
        {
            Multiply(in quaternion, scale, out quaternion);
            return quaternion;
        }

        /// <summary>
        /// Multiplies an instance by a scalar.
        /// </summary>
        /// <param name="quaternion">The instance.</param>
        /// <param name="scale">The scalar.</param>
        /// <returns>A new instance containing the result of the calculation.</returns>
        [Pure]
        public static QuaternionD operator *(double scale, QuaternionD quaternion) => new(
                quaternion.X * scale,
                quaternion.Y * scale,
                quaternion.Z * scale,
                quaternion.W * scale
            );

        /// <summary>
        /// Compares two instances for equality.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>True, if left equals right; false otherwise.</returns>
        public static bool operator ==(QuaternionD left, QuaternionD right) => left.Equals(right);

        /// <summary>
        /// Compares two instances for inequality.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>True, if left does not equal right; false otherwise.</returns>
        public static bool operator !=(QuaternionD left, QuaternionD right) => !(left == right);

        /// <inheritdoc />
        public override readonly bool Equals(object? obj) => obj is QuaternionD quaternion && Equals(quaternion);

        /// <inheritdoc />
        public readonly bool Equals(QuaternionD other) => Xyz.Equals(other.Xyz) &&
                   W == other.W;

        /// <inheritdoc />
#pragma warning disable S2328 // "GetHashCode" should not reference mutable fields
        public override readonly int GetHashCode() => HashCode.Combine(Xyz, W);

        /// <summary>
        /// Returns a System.String that represents the current Quaterniond.
        /// </summary>
        /// <returns>A human-readable representation of the quaternion.</returns>
        public override readonly string ToString() => ToString(null, null);

        /// <inheritdoc cref="ToString(string, IFormatProvider)"/>
        public readonly string ToString(string format) => ToString(format, null);

        /// <inheritdoc cref="ToString(string, IFormatProvider)"/>
        public readonly string ToString(IFormatProvider formatProvider) => ToString(null, formatProvider);

        /// <inheritdoc/>
        public readonly string ToString(string? format, IFormatProvider? formatProvider)
        {
            string ls = MathHelper.GetListSeparator(formatProvider);
            string xyz = Xyz.ToString(format, formatProvider);
            string w = W.ToString(format, formatProvider);
            return $"V: {xyz}{ls} W: {w}";
        }
    }
}
