using Entities.Exceptions.Base;

namespace Entities.Exceptions {
    public class SmptConfigurationNotFoundException(string configurationName) 
        : NotFoundException($"Email configuration with name {configurationName} does not exists in configurations.") {
    }
}
