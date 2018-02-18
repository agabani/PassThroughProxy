# Pass Through Proxy

> Forward proxy server.

[![Build status](https://ci.appveyor.com/api/projects/status/c6gnmw6xkt6j8e2s?svg=true)](https://ci.appveyor.com/project/AhmedAgabani/passthroughproxy)

## Usage

Double click `Proxy.exe` or run from command line.
Point your browser/system proxy settings to 127.0.0.1 port 8889

## Features
* HTTP proxy
* HTTPS tunneling
* Proxy Authentication
* Firewall

## Configuration
Configuration can be found in the `config.json` file that comes packaged with the proxy.

| Configuration Path       | Description                                      | Type    | Example     |
|--------------------------|--------------------------------------------------|---------|-------------|
| server.port              | Port the proxy will listen on                    | int     | 8889        |
|                          |                                                  |         |             |
| authentication.enabled   | Enable proxy authentication                      | bool    | false       |
| authentication.username  | Username used to authenticate                    | string  | john.smith  |
| authentication.password  | Password used to authenticate                    | string  | password123 |
|                          |                                                  |         |             |
| firewall.enabled         | Enables the firewall                             | bool    | true        |
| firewall.rules[].pattern | Rule regex pattern to match against the hostname | string  | .*bbc.co.uk |
| firewall.rules[].action  | Action to take when hostname matches rule        | string  | deny        |

### Example

```json
{
  "server": {
    "port": 8889
  },
  "authentication": {
    "enabled": false,
    "username": "username",
    "password": "password"
  },
  "firewall": {
    "enabled": true,
    "rules": [
      {
        "pattern": ".*reddit.com",
        "action": "allow"
      },
      {
        "pattern": ".*bbc.co.uk",
        "action": "deny"
      }
    ]
  }
}
```

## License

```
MIT License

Copyright (c) 2016 Ahmed Agabani

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```
