using Microsoft.DurableTask;
using Niobium.AI.Ecommerce.Tools;

namespace Niobium.AI.Ecommerce.Workflows
{
    internal record PersonalDataRequest(string TargetCountry, int Count);

    [DurableTask]
    internal class GeneratePersonalData : TaskActivity<PersonalDataRequest, IReadOnlyList<FirstNameCityPair>>
    {
        public override Task<IReadOnlyList<FirstNameCityPair>> RunAsync(TaskActivityContext context, PersonalDataRequest input)
            => Task.FromResult(LocalizedPersonDataHelper.GenerateFirstNameCityPairs(input.TargetCountry, input.Count));
    }
}
