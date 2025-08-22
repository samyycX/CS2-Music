# CS2-Music
基于 CSSharp 的 CS2 点歌插件。

> [!CAUTION]
> 网易云曾对使用第三方API的账号进行封禁。使用此插件则代表您已明知您的网易云账号被冻结或封禁的风险。

# 功能
- 游戏内点歌
- ~~HUD 歌词~~（因更新移除）
- 点歌花费积分
- 个人歌词/声音开关控制
- 管理员控制

# 依赖
- [Audio](https://github.com/samyycX/Audio)
- [cs2-store](https://github.com/schwarper/cs2-store) (可选，如果需要积分控制则需要安装)

# 命令
- `!music`: 打开点歌菜单 (包括管理员控制菜单)
- `!musicreload`: 重载配置文件 (需要 `@css/admin` 权限)

# 配置项
### `General`
- `AdminFlag`: 管理员控制需要的权限
- `Volume`: 全局歌曲音量
- `LyricInterval`: 歌词更新一次的间隔，与 Audio 插件发包的速度相同，单位为ms，如果出现歌词过快/过慢现象需手动微调
- `Price`: 点歌需要的积分，如果设置为0则不需要安装 store 插件
- `RefundRate`: 当一首歌因用户自己取消或意外终止后退还的积分比例
- `Debug`: 启用控制台debug

### `MusicApi`
- `NeteaseMusicCookie`: 网易云的 `MUSIC_U` cookie的值，可在网页请求头的cookie内找到，样例: `00AAA7B4B42AC9CC20A9AB45D236FB2A547578EXXXXXXXXX` (很长)

# Credits
- 感谢 [@ELDment](https://github.com/ELDment) 提供的网易云 API 支持。([CSharp-Music-API](https://github.com/ELDment/CSharp-Music-API))
