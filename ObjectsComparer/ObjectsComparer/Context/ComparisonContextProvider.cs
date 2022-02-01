﻿using System;
using System.Reflection;

namespace ObjectsComparer
{
    internal static class ComparisonContextProvider
    {
        internal static IComparisonContext CreateImplicitRootContext(ComparisonSettings comparisonSettings)
        {
            _ = comparisonSettings ?? throw new ArgumentNullException(nameof(comparisonSettings)); //For the future.

            return new NullComparisonContext(ComparisonContextMember.Create());
        }              

        /// <summary>
        /// Context with ancestor but without a member.
        /// </summary>
        public static IComparisonContext CreateListElementContext(ComparisonSettings comparisonSettings, IComparisonContext ancestor)
        {
            _ = comparisonSettings ?? throw new ArgumentNullException(nameof(comparisonSettings));
            _ = ancestor ?? throw new ArgumentNullException(nameof(ancestor));

            return CreateContext(comparisonSettings, ComparisonContextMember.Create(), ancestor);
        }

        /// <summary>
        /// Context with ancestor and member.
        /// </summary>
        public static IComparisonContext CreateMemberContext(ComparisonSettings comparisonSettings, IComparisonContext ancestor, MemberInfo member)
        {
            _ = comparisonSettings ?? throw new ArgumentNullException(nameof(comparisonSettings));
            _ = ancestor ?? throw new ArgumentNullException(nameof(ancestor));
            _ = member ?? throw new ArgumentNullException(nameof(member));

            return CreateContext(comparisonSettings, ComparisonContextMember.Create(member), ancestor);
        }

        /// <summary>
        /// Context with ancestor and member.
        /// </summary>
        public static IComparisonContext CreateMemberNameContext(ComparisonSettings comparisonSettings, IComparisonContext ancestor, string memberName)
        {
            _ = comparisonSettings ?? throw new ArgumentNullException(nameof(comparisonSettings));
            _ = ancestor ?? throw new ArgumentNullException(nameof(ancestor));
            _ = memberName ?? throw new ArgumentNullException(nameof(memberName));

            return CreateContext(comparisonSettings, ComparisonContextMember.Create(memberName), ancestor);
        }

        /// <summary>
        /// Context with ancestor and member. The <paramref name="member"/> takes precedence over <paramref name="memberName"/>.
        /// </summary>
        internal static IComparisonContext CreateMemberOrMemberNameContext(ComparisonSettings comparisonSettings, IComparisonContext ancestor, MemberInfo member, string memberName)
        {
            _ = comparisonSettings ?? throw new ArgumentNullException(nameof(comparisonSettings));
            _ = ancestor ?? throw new ArgumentNullException(nameof(ancestor));

            if (member != null)
            {
                return CreateMemberContext(comparisonSettings, ancestor, member);
            }

            if (memberName != null)
            {
                return CreateMemberNameContext(comparisonSettings, ancestor, memberName);
            }

            throw new ArgumentException();
        }

        static IComparisonContext CreateContext(ComparisonSettings comparisonSettings, IComparisonContextMember comparisonContextMember, IComparisonContext ancestor)
        {
            _ = comparisonSettings ?? throw new ArgumentNullException(nameof(comparisonSettings));
            _ = comparisonContextMember ?? throw new ArgumentNullException(nameof(comparisonContextMember));
            _ = ancestor ?? throw new ArgumentNullException(nameof(ancestor));

            ComparisonContextOptions options = ComparisonContextOptions.Default();
            comparisonSettings.ComparisonContextOptionsAction?.Invoke(ancestor, options);

            if (options.ComparisonContextMemberFactory != null)
            {
                var customComparisonContextMember = options.ComparisonContextMemberFactory.Invoke(comparisonContextMember);
                
                if (customComparisonContextMember == null)
                {
                    throw new InvalidOperationException("Comparison context member factory returned null member.");
                }

                comparisonContextMember = customComparisonContextMember;
            }

            if (options.ComparisonContextFactory != null)
            {
                var customContext = options.ComparisonContextFactory(comparisonContextMember);

                if (customContext != null)
                {
                    return customContext;
                }

                if (customContext == null)
                {
                    throw new InvalidOperationException("Comparison context factory returned null context.");
                }
            }

            return new ComparisonContext(comparisonContextMember, ancestor);
        }
    }
}
