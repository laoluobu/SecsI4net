
# secsi4net

 [![Nuget](https://img.shields.io/nuget/dt/SecsI4net)](https://www.nuget.org/stats/packages/SecsI4net?groupby=Version) [![NuGet](https://img.shields.io/nuget/v/SecsI4net.svg)](https://www.nuget.org/packages/SecsI4net)


 **Project Description**  

SECS-I/GEM implementation on .NET . base on [Secs4Net](https://github.com/mkjeff/secs4net/)

**Getting started**

## Install Nuget package

    > dotnet add package SecsI4Net

## Quick Start


```csharp
var secei = new SecsIConnector("COM1", OnMessageReceive);

string s2f42 = ":'S2F41' W  \n" +
                            "<L[2]\n" +
                            "  <A[4] \"UNLOCK\">\n" +
                            "  <L[0]\n" +
                            "  >\n" +
                            ">\n" +
                            ".";

secei.SendAsync(s2f42.ToSecsMessage());

private void OnMessageReceive(SecsMessage message){


}

```