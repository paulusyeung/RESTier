﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.AspNet.OData;
using Microsoft.OData.Edm;
using Microsoft.Restier.Core;
using Microsoft.Restier.Core.Model;

#if NETCOREAPP3_1_OR_GREATER
namespace Microsoft.Restier.AspNetCore.Model
#else
namespace Microsoft.Restier.AspNet.Model
#endif
{
    /// <summary>
    /// Represents a model mapper based on a DbContext.
    /// </summary>
    public class RestierWebApiModelMapper : IModelMapper
    {

        /// <summary>
        /// Gets or sets the inner mapper.
        /// </summary>
        internal IModelMapper InnerMapper { get; set; }

        /// <summary>
        /// Tries to get the relevant type of an entity
        /// set, singleton, or composable function import.
        /// </summary>
        /// <param name="context">The context for model mapper.</param>
        /// <param name="name">The name of an entity set, singleton or composable function import.</param>
        /// <param name="relevantType">When this method returns, provides the relevant type of the queryable source.</param>
        /// <returns>
        /// <c>true</c> if the relevant type was provided; otherwise, <c>false</c>.
        /// </returns>
        public bool TryGetRelevantType(ModelContext context, string name,  out Type relevantType)
        {
            Ensure.NotNull(context, nameof(context));

            var model = context.Api.GetModel();

            var element = model.EntityContainer.Elements.Where(e => e.Name == name).FirstOrDefault();

            if (element is not null)
            {
                IEdmType entityType = null;
                if (element is EdmEntitySet entitySet)
                {
                    var entitySetType = entitySet.Type as EdmCollectionType;
                    entityType = entitySetType.ElementType.Definition;
                }
                else
                {
                    if (element is EdmSingleton singleton)
                    {
                        entityType = singleton.Type;
                    }
                }

                if (entityType is not null)
                {
                    var annotation = model.GetAnnotationValue<ClrTypeAnnotation>(entityType);
                    if (annotation is not null)
                    {
                        relevantType = annotation.ClrType;
                        return true;
                    }
                }
            }

            return InnerMapper.TryGetRelevantType(context, name, out relevantType);
        }

        /// <summary>
        /// Tries to get the relevant type of a composable function.
        /// </summary>
        /// <param name="context">The context for model mapper.</param>
        /// <param name="namespaceName">The name of a namespace containing a composable function.</param>
        /// <param name="name">The name of composable function.</param>
        /// <param name="relevantType">When this method returns, provides the relevant type of the composable function.</param>
        /// <returns>
        /// <c>true</c> if the relevant type was provided; otherwise, <c>false</c>.
        /// </returns>
        public bool TryGetRelevantType(ModelContext context, string namespaceName, string name, out Type relevantType)
        {
            // TODO GitHubIssue#39 : support composable function imports
            // relevantType = null;
            return InnerMapper.TryGetRelevantType(context, namespaceName, name, out relevantType);
        }
    }
}
