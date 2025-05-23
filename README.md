<img alt="Coolapk LOGO" src="./logo.png" width="200px" />

# Coolapk Lite
一个基于UWP平台的酷安客户端精简版

[![Build Status](https://dev.azure.com/wherewhere/Coolapk-UWP/_apis/build/status/Coolapk-UWP.Coolapk-Lite?branchName=master)](https://dev.azure.com/wherewhere/Coolapk-UWP/_build/latest?definitionId=5&branchName=master "Build Status")

[![LICENSE](https://img.shields.io/github/license/Coolapk-UWP/Coolapk-Lite.svg?label=License&style=flat-square)](https://github.com/Coolapk-UWP/Coolapk-Lite/blob/master/LICENSE "LICENSE")
[![Issues](https://img.shields.io/github/issues/Coolapk-UWP/Coolapk-Lite.svg?label=Issues&style=flat-square)](https://github.com/Coolapk-UWP/Coolapk-Lite/issues "Issues")
[![Stargazers](https://img.shields.io/github/stars/Coolapk-UWP/Coolapk-Lite.svg?label=Stars&style=flat-square)](https://github.com/Coolapk-UWP/Coolapk-Lite/stargazers "Stargazers")

[![Microsoft Store](https://img.shields.io/badge/download-下载-magenta.svg?label=Microsoft%20Store&logo=data:image/svg+xml;base64,PHN2ZyByb2xlPSJpbWciIHZpZXdCb3g9IjAgMCAyNCAyNCIgZmlsbD0iI2ZmZiIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj48dGl0bGU+TWljcm9zb2Z0IFN0b3JlPC90aXRsZT48cGF0aCBkPSJNMTEuNCA5LjZ2NC4ySDcuMlY5LjZoNC4yem0wIDkuNlYxNUg3LjJ2NC4yaDQuMnptNS40LTkuNnY0LjJoLTQuMlY5LjZoNC4yem0wIDkuNlYxNWgtNC4ydjQuMmg0LjJ6TTcuMiA1LjRWMi43YzAtMS4xNi45NC0yLjEgMi4xLTIuMWg1LjRjMS4xNiAwIDIuMS45NCAyLjEgMi4xdjIuN2g2LjNhLjkuOSAwIDAgMSAuOS45djEzLjhhMy4zIDMuMyAwIDAgMS0zLjMgMy4zSDMuM0EzLjMgMy4zIDAgMCAxIDAgMjAuMVY2LjNhLjkuOSAwIDAgMSAuOS0uOWg2LjN6TTkgMi43djIuN2g2VjIuN2EuMy4zIDAgMCAwLS4zLS4zSDkuM2EuMy4zIDAgMCAwLS4zLjN6TTEuOCAyMC4xYTEuNSAxLjUgMCAwIDAgMS41IDEuNWgxNy40YTEuNSAxLjUgMCAwIDAgMS41LTEuNVY3LjJIMS44djEyLjl6Ii8+PC9zdmc+&style=for-the-badge&color=11a2f8)](https://www.microsoft.com/store/apps/9NB8J1BH0D7T "Microsoft Store")
[![GitHub All Releases](https://img.shields.io/github/downloads/Coolapk-UWP/Coolapk-Lite/total.svg?label=DOWNLOAD&logo=github&style=for-the-badge)](https://github.com/Coolapk-UWP/Coolapk-Lite/releases/latest "GitHub All Releases")

## 目录
- [Coolapk Lite](#coolapk-lite)
  - [目录](#目录)
  - [如何安装应用](#如何安装应用)
    - [最低需求](#最低需求)
    - [使用应用安装脚本安装应用](#使用应用安装脚本安装应用)
    - [使用应用安装程序安装应用](#使用应用安装程序安装应用)
    - [更新应用](#更新应用)
  - [使用到的模块](#使用到的模块)
  - [衍生项目](#衍生项目)
  - [参与人员](#参与人员)
  - [鸣谢](#鸣谢)

## 如何安装应用
### 最低需求
- Windows 10 Build 10240及以上
- 设备需支持ARM/ARM64/x86/x64
- 至少200MB的空余储存空间(用于储存安装包与安装应用)

### 使用应用安装脚本安装应用
- 下载并解压最新的[安装包`(CoolapkLite_x.x.x.0_Test.rar)`](https://github.com/Coolapk-UWP/Coolapk-Lite/releases/latest)
- 如果没有应用安装脚本，下载[`Install.ps1`](Install.ps1)到目标目录
![Install.ps1](Images/Guides/Snipaste_2019-10-12_22-49-11.png)
- 右击`Install.ps1`，选择“使用PowerShell运行”
- 应用安装脚本将会引导您完成此过程的剩余部分

### 使用应用安装程序安装应用
- 下载并解压最新的[安装包`(CoolapkLite_x.x.x.0_Test.rar)`](https://github.com/Coolapk-UWP/Coolapk-Lite/releases/latest)
- [开启旁加载模式](https://www.windowscentral.com/how-enable-windows-10-sideload-apps-outside-store)
  - 如果您想开发UWP应用，您可以开启[开发人员模式](https://docs.microsoft.com/zh-cn/windows/uwp/get-started/enable-your-device-for-development)，**对于大多数不需要做UWP开发的用户来说，开发人员模式是没有必要的**
- 安装`Dependencies`文件夹下的适用于您的设备的所有依赖包
![Dependencies](Images/Guides/Snipaste_2019-10-13_15-51-33.png)
- 安装`*.cer`证书到`本地计算机`→`受信任的根证书颁发机构`
  - 这项操作需要用到管理员权限，如果您安装证书时没有用到该权限，则可能是因为您将证书安装到了错误的位置或者您使用的是超级管理员账户
  ![安装证书](Images/Guides/Snipaste_2019-10-12_22-46-37.png)
  ![导入本地计算机](Images/Guides/Snipaste_2019-10-19_15-28-58.png)
  ![储存到受信任的根证书颁发机构](Images/Guides/Snipaste_2019-10-20_23-36-44.png)
- 双击`*.appxbundle`，单击安装，坐和放宽
![安装](Images/Guides/Snipaste_2019-10-13_12-42-40.png)

### 更新应用
- 下载并解压最新的[安装包`(CoolapkLite_x.x.x.0_x86_x64_arm.appxbundle)`](https://github.com/Coolapk-UWP/Coolapk-Lite/releases/latest)
- 双击`*.appxbundle`，单击安装，坐和放宽
![安装](Images/Guides/Snipaste_2019-10-13_16-01-09.png)

## 使用到的模块
- [QRCoder](https://github.com/codebude/QRCoder "QRCoder")
- [MetroLog](https://github.com/novotnyllc/MetroLog "MetroLog")
- [Bcrypt.Net](https://github.com/BcryptNet/bcrypt.net "Bcrypt.Net")
- [Newtonsoft Json](https://www.newtonsoft.com/json "Newtonsoft Json")
- [Windows Community Toolkit](https://github.com/CommunityToolkit/WindowsCommunityToolkit "Windows Community Toolkit")

## 衍生项目
- [Coolapk-API-Collect](https://github.com/Coolapk-UWP/Coolapk-API-Collect "Coolapk-API-Collect")

## 参与人员
[![Contributors](https://contrib.rocks/image?repo=Coolapk-UWP/Coolapk-Lite)](https://github.com/Coolapk-UWP/Coolapk-Lite/graphs/contributors "Contributors")

## 鸣谢
- 酷安UWP原作者[@一块小板子](http://www.coolapk.com/u/695942 "一块小板子")([Github](https://github.com/oboard "oboard"))
- OpenCoolapk作者[@roykio](http://www.coolapk.com/u/703542 "roykio")([Github](https://github.com/roykio "roykio"))
- CoolapkTokenCrack作者[@ZCKun](http://www.coolapk.com/u/654147 "ZCKun")([Github](https://github.com/ZCKun "0x2h"))
- Coolapk-kotlin作者[@bjzhou](http://www.coolapk.com/u/528097 "bjzhou")([Github](https://github.com/bjzhou "hinnka"))
- 以及所有为酷安UWP项目做出贡献的同志们
- **铺路尚未成功，同志仍需努力！**

## Star 数量统计
[![Star 数量统计](https://starchart.cc/Coolapk-UWP/Coolapk-Lite.svg?variant=adaptive)](https://github.com/Coolapk-UWP/Coolapk-Lite/stargazers "Star 数量统计")
