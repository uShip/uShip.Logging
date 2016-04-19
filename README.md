[![AppVeyor](https://ci.appveyor.com/api/projects/status/github/uShip/uShip.Logging?svg=true)](https://ci.appveyor.com/project/uShip/uShip-Logging)
[![NuGet version](https://badge.fury.io/nu/uship.logging.svg)](https://www.nuget.org/packages/uship.logging)

# uShip.Logging
Opinionated fluent library for logging to [Logstash](https://github.com/elastic/logstash) and [Graphite](https://github.com/mit-carbon/Graphite) based on [log4net](https://github.com/apache/log4net).

## Basic Usage

### Logstash
```csharp
var logger = Logger.GetInstance();

// log plain message with severity WARN
logger
.Message("This is a warning.").
.Warn()
.Write();

// log exception with custom data
var exception = new Exception();
logger
.Exception(exception)
.Data("userId", 1027)
.Write();
```

### Graphite
```csharp
// Implement IGraphiteKey interface
public class GraphiteKey : IGraphiteKey
{
  public static GraphiteKey AccountCreated = new GraphiteKey("AccountCreated");

  private readonly string _key;

  private GraphiteKey(string key)
  {
      _key = key;
  }

  public string Key { get { return _key; } }
}


var logger = Logger.GetInstance();

// count
logger.Write(GraphiteKey.AccountCreated, null /*Optional SubKey*/);

// timing
logger.Write(GraphiteKey.AccountCreated, null /*Optional SubKey*/, 100 /*milliseconds*/)
```

## Configuration Settings
### Custom Configuration Section
uShip.Logging exposes the following configuration settings via a custom configuration section.

| Setting | Description |
|---------|-------------|
| graphiteMetricPath | The prefix of all metrics that are logged to graphite |
| minimalDataLogMessage | The message to log when calling .WriteMinimalDataLog |
| jsonReplacements | The names of JSON fields that should be sanitized and not logged such as passwords and credit card numbers |
| urlFormEncodedReplacements | The names of form fields that should sanitized and not logged such as passwords and credit card numbers |
| regexReplacements | Regex patterns for fields that should be sanitized and not logged that do not fit into the JSON or URL encoded category above |
| targetStackTraces | Stacktraces that you designate as the root causes of any exception. These values are usually the root namespaces of your project |

### log4net configuration

In addition to the custom configuration section, uShip.Logging depends on the following loggers to be configured

| Logger | Purpose |
|--------|---------|
| Logstash | All exceptions and messages written via .Exception and .Message will be sent to this logger |
| Graphite | All graphite metrics logged will be sent to this logger |
| Minimal | All messages written via WriteMinimalDataLog will be sent to this logger |

The following appenders are available in addition to the appenders provided by log4net

| Appender | Purpose | Relevant Settings |
|----------|---------|-------------------|
| uShip.Logging.Appender.LogstashAppender | Writes messages to Logstash | remoteAddress, remotePort |
| uShip.Logging.Appender.ApiErrorLogAppender | Writes messages to an API as a webhook | apiUrl |
| uShip.Logging.Appender.FileLogAppender | Writes messages to disk | baseDirectory |

### Example Configuration

See an example App.config [here](https://github.com/uShip/uShip.Logging/blob/master/src/uShip.Logging.Tests/App.config).

## Contributing
See [CONTRIBUTING.md](CONTRIBUTING.md)