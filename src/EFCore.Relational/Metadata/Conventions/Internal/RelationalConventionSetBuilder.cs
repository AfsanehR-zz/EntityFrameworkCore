// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    /// <summary>
    ///     <para>
    ///         This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///         directly from your code. This API may change or be removed in future releases.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped"/> and multiple registrations
    ///         are allowed. This means that each <see cref="DbContext"/> instance will use its own
    ///         set of instances of this service.
    ///         The implementations may depend on other services registered with any lifetime.
    ///         The implementations do not need to be thread-safe.
    ///     </para>
    /// </summary>
    // Issue#11266 This type is being used by provider code. Do not break.
    public abstract class RelationalConventionSetBuilder : IConventionSetBuilder
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RelationalConventionSetBuilder" /> class.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        protected RelationalConventionSetBuilder([NotNull] RelationalConventionSetBuilderDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            Dependencies = dependencies;
        }

        /// <summary>
        ///     Parameter object containing service dependencies.
        /// </summary>
        protected virtual RelationalConventionSetBuilderDependencies Dependencies { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConventionSet AddConventions(ConventionSet conventionSet)
        {
            var logger = Dependencies.Logger;

            ValueGeneratorConvention valueGeneratorConvention = new RelationalValueGeneratorConvention(logger);

            ReplaceConvention(conventionSet.BaseEntityTypeChangedConventions, valueGeneratorConvention);
            ReplaceConvention(conventionSet.PrimaryKeyChangedConventions, valueGeneratorConvention);
            ReplaceConvention(conventionSet.ForeignKeyAddedConventions, valueGeneratorConvention);
            ReplaceConvention(conventionSet.ForeignKeyRemovedConventions, valueGeneratorConvention);

            var relationalColumnAttributeConvention = new RelationalColumnAttributeConvention(logger);

            conventionSet.PropertyAddedConventions.Add(relationalColumnAttributeConvention);

            var sharedTableConvention = new SharedTableConvention(logger);

            var discriminatorConvention = new DiscriminatorConvention(logger);
            conventionSet.EntityTypeAddedConventions.Add(new RelationalTableAttributeConvention(logger));
            conventionSet.EntityTypeRemovedConventions.Add(discriminatorConvention);
            conventionSet.BaseEntityTypeChangedConventions.Add(discriminatorConvention);
            conventionSet.BaseEntityTypeChangedConventions.Add(
                new TableNameFromDbSetConvention(Dependencies.Context?.Context, Dependencies.SetFinder, logger));
            conventionSet.PropertyFieldChangedConventions.Add(relationalColumnAttributeConvention);
            conventionSet.PropertyAnnotationChangedConventions.Add((RelationalValueGeneratorConvention)valueGeneratorConvention);

            conventionSet.ModelBuiltConventions.Add(sharedTableConvention);

            conventionSet.ModelAnnotationChangedConventions.Add(new RelationalDbFunctionConvention(logger));

            return conventionSet;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual void ReplaceConvention<T1, T2>([NotNull] IList<T1> conventionsList, [NotNull] T2 newConvention)
            where T2 : T1
        {
            var oldConvention = conventionsList.OfType<T2>().FirstOrDefault();
            if (oldConvention == null)
            {
                throw new InvalidOperationException();
            }

            var index = conventionsList.IndexOf(oldConvention);
            conventionsList.RemoveAt(index);
            conventionsList.Insert(index, newConvention);
        }
    }
}
