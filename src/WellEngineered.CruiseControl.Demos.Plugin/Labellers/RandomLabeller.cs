using System;

using WellEngineered.CruiseControl.Core;
using WellEngineered.CruiseControl.Core.Config;
using WellEngineered.CruiseControl.Core.Label;
using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Attributes;

namespace WellEngineered.CruiseControl.Demos.Plugin.Labellers
{
    [ReflectorType("randomLabeller")]
    public class RandomLabeller
        : ILabeller, IConfigurationValidation
    {
        public RandomLabeller()
        {
            this.MaximumValue = int.MaxValue;
        }

        [ReflectorProperty("max", Required = false)]
        public int MaximumValue { get; set; }

        public string Generate(IIntegrationResult integrationResult)
        {
            var rand = new Random();
            var label = rand.Next(this.MaximumValue).ToString();
            return label;
        }

        public void Run(IIntegrationResult result)
        {
            result.Label = this.Generate(result);
        }

        public void Validate(IConfiguration configuration,
            ConfigurationTrace parent,
            IConfigurationErrorProcesser errorProcesser)
        {
            if (this.MaximumValue <= 0)
            {
                errorProcesser.ProcessError(
                    "The maximum value must be greater than zero");
            }
        }
    }
}
