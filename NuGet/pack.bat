@echo off
nuget pack MiniProfilerFody.nuspec -properties version=%1
nuget push -source d:\work\packages MiniProfiler.Fody.%1.nupkg
@pause