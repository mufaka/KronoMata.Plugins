# KronoMata.Plugins
Plugin Packs for KronoMata

### Test.KronoMata.Plugins.Network Dependency
For the HttpPlugin I am using httpbin to verify requests. I didn't want to keep sending requests to httpbin.org so I am using a Docker installation for it. The url is configured at the top of the HttpPluginTests.cs file.

My configuration:

    docker pull kennethreitz/httpbin
    docker run -p 88:80 kennethreitz/httpbin
