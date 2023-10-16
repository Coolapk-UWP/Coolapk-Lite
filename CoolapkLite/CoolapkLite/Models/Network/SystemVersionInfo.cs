﻿using System;
using Windows.ApplicationModel;

namespace CoolapkLite.Models.Network
{
    public readonly struct SystemVersionInfo
    {
        public SystemVersionInfo(int major, int minor, int build, int revision = 0)
        {
            Major = major;
            Minor = minor;
            Build = build;
            Revision = revision;
        }

        public int Major { get; }

        public int Minor { get; }

        public int Build { get; }

        public int Revision { get; }

        public bool Equals(SystemVersionInfo other) => Major == other.Major && Minor == other.Minor && Build == other.Build && Revision == other.Revision;

        public override bool Equals(object obj) => obj is SystemVersionInfo other && Equals(other);

        public override int GetHashCode() => Major.GetHashCode() ^ Minor.GetHashCode() ^ Build.GetHashCode() ^ Revision.GetHashCode();

        public static bool operator ==(SystemVersionInfo left, SystemVersionInfo right) => left.Equals(right);

        public static bool operator !=(SystemVersionInfo left, SystemVersionInfo right) => !(left == right);

        public int CompareTo(SystemVersionInfo other) =>
            Major != other.Major
                ? Major.CompareTo(other.Major)
                : Minor != other.Minor
                ? Minor.CompareTo(other.Minor)
                : Build != other.Build ? Build.CompareTo(other.Build) : Revision != other.Revision ? Revision.CompareTo(other.Revision) : 0;

        public int CompareTo(object obj) => obj is SystemVersionInfo other ? CompareTo(other) : throw new ArgumentException();

        public static bool operator <(SystemVersionInfo left, SystemVersionInfo right) => left.CompareTo(right) < 0;

        public static bool operator <=(SystemVersionInfo left, SystemVersionInfo right) => left.CompareTo(right) <= 0;

        public static bool operator >(SystemVersionInfo left, SystemVersionInfo right) => left.CompareTo(right) > 0;

        public static bool operator >=(SystemVersionInfo left, SystemVersionInfo right) => left.CompareTo(right) >= 0;

        /// <summary>
        /// Returns a string representation of a version with the format 'Major.Minor.Build.Revision'.
        /// </summary>
        /// <param name="packageVersion">The <see cref="PackageVersion"/> to convert to a string</param>
        /// <param name="significance">The number of version numbers to return, default is 4 for the full version number.</param>
        /// <returns>Version string of the format 'Major.Minor.Build.Revision'</returns>
        /// <example>
        /// Package.Current.Id.Version.ToFormattedString(2); // Returns "7.0" for instance.
        /// </example>
        public string ToString(int significance = 4)
        {
            switch (significance)
            {
                case 4:
                    return $"{Major}.{Minor}.{Build}.{Revision}";
                case 3:
                    return $"{Major}.{Minor}.{Build}";
                case 2:
                    return $"{Major}.{Minor}";
                case 1:
                    return $"{Major}";
            }

            string ThrowArgumentOutOfRangeException() => throw new ArgumentOutOfRangeException(nameof(significance), "Value must be a value 1 through 4.");

            return ThrowArgumentOutOfRangeException();
        }

        public override string ToString() => $"{Major}.{Minor}.{Build}.{Revision}";

        public static implicit operator SystemVersionInfo(Version version) => new SystemVersionInfo(version.Major, version.Minor, version.Build, version.Revision);

        public static implicit operator SystemVersionInfo(PackageVersion version) => new SystemVersionInfo(version.Major, version.Minor, version.Build, version.Revision);

        public static implicit operator Version(SystemVersionInfo version) => new Version(version.Major, version.Minor, version.Build, version.Revision);

        public static explicit operator PackageVersion(SystemVersionInfo version) => new PackageVersion { Major = (ushort)version.Major, Minor = (ushort)version.Minor, Build = (ushort)version.Build, Revision = (ushort)version.Revision };
    }
}
