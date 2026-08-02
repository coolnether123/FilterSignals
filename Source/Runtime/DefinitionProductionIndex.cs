using System.Collections.Generic;
using System.Linq;
using Verse;

namespace FilterSignals.Runtime
{
    internal sealed class DefinitionProductionIndex
    {
        private readonly Dictionary<ThingDef, List<RecipeDef>> recipesByProduct;
        private readonly Dictionary<RecipeDef, ThingDef[]> sourcesByRecipe;
        private readonly HashSet<ThingDef> productionSourceDefs;

        private DefinitionProductionIndex(
            Dictionary<ThingDef, List<RecipeDef>> recipesByProduct,
            Dictionary<RecipeDef, ThingDef[]> sourcesByRecipe,
            HashSet<ThingDef> productionSourceDefs)
        {
            this.recipesByProduct = recipesByProduct;
            this.sourcesByRecipe = sourcesByRecipe;
            this.productionSourceDefs = productionSourceDefs;
        }

        internal IReadOnlyList<RecipeDef> RecipesFor(ThingDef product)
        {
            return product != null &&
                recipesByProduct.TryGetValue(product, out List<RecipeDef> recipes)
                    ? recipes
                    : EmptyRecipes;
        }

        internal bool IsProductionSource(ThingDef thingDef)
        {
            return thingDef != null && productionSourceDefs.Contains(thingDef);
        }

        internal IReadOnlyList<ThingDef> SourcesFor(RecipeDef recipe)
        {
            return recipe != null &&
                sourcesByRecipe.TryGetValue(recipe, out ThingDef[] sources)
                    ? sources
                    : EmptySources;
        }

        internal static DefinitionProductionIndex Build()
        {
            var recipesByProduct =
                new Dictionary<ThingDef, List<RecipeDef>>();
            var sourcesByRecipe =
                new Dictionary<RecipeDef, ThingDef[]>();
            var productionSourceDefs = new HashSet<ThingDef>();

            foreach (RecipeDef recipe in
                DefDatabase<RecipeDef>.AllDefsListForReading)
            {
                if (recipe == null ||
                    recipe.IsSurgery ||
                    recipe.products == null ||
                    recipe.products.Count == 0)
                {
                    continue;
                }

                ThingDef[] sourceDefs = recipe.AllRecipeUsers
                    .Where(source => source != null &&
                        source.category == ThingCategory.Building)
                    .Distinct()
                    .OrderBy(
                        source => source.defName,
                        System.StringComparer.Ordinal)
                    .ToArray();
                sourcesByRecipe[recipe] = sourceDefs;
                foreach (ThingDef sourceDef in sourceDefs)
                {
                    productionSourceDefs.Add(sourceDef);
                }

                foreach (ThingDefCountClass product in recipe.products)
                {
                    if (product?.thingDef == null)
                    {
                        continue;
                    }

                    if (!recipesByProduct.TryGetValue(
                        product.thingDef,
                        out List<RecipeDef> recipes))
                    {
                        recipes = new List<RecipeDef>();
                        recipesByProduct.Add(product.thingDef, recipes);
                    }

                    if (!recipes.Contains(recipe))
                    {
                        recipes.Add(recipe);
                    }
                }
            }

            foreach (List<RecipeDef> recipes in recipesByProduct.Values)
            {
                recipes.Sort((left, right) =>
                    string.Compare(
                        left.defName,
                        right.defName,
                        System.StringComparison.Ordinal));
            }

            return new DefinitionProductionIndex(
                recipesByProduct,
                sourcesByRecipe,
                productionSourceDefs);
        }

        private static readonly IReadOnlyList<RecipeDef> EmptyRecipes =
            new RecipeDef[0];
        private static readonly IReadOnlyList<ThingDef> EmptySources =
            new ThingDef[0];
    }
}
