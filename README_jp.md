# <img src="Assets/rounder.png" width="40" align="center" /> Rounder for Windows

Windows 10 / 11 の画面の角を、自然な角丸にするネイティブユーティリティです。

[![Latest release](https://img.shields.io/github/v/release/nisesimadao/Rounder_Windows?label=download)](https://github.com/nisesimadao/Rounder_Windows/releases/latest)
[![Build & Release](https://github.com/nisesimadao/Rounder_Windows/actions/workflows/release.yml/badge.svg)](https://github.com/nisesimadao/Rounder_Windows/actions/workflows/release.yml)
[![Windows](https://img.shields.io/badge/Windows-10%20%2F%2011-0078D4)](#システム要件)

<p align="center">
  <img src="docs/assets/rounder-windows-overview.png" alt="Rounder for Windows 設定画面とトレイ操作" width="900" />
</p>

四角い外部モニターや、角丸のないディスプレイに小さなクリック透過オーバーレイを重ね、Windows自体を改造せずに角を自然に丸く見せます。普段は通知領域から操作でき、必要ならトレイアイコンを隠したままバックグラウンドで動かせます。**画面収録・アクセシビリティ・管理者権限・ネットワーク接続は不要**です。

[English README](./README.md)

## 主な機能

- **3種類の角形状** — Rounded / Squircle / Polygon
- **0〜40pxの角半径** — トレイメニューからライブ調整
- **任意の角色** — 黒 / 白 / グレーのクイック選択付き
- **四隅を個別にオン/オフ**
- **マルチディスプレイ対応** — Rounderを適用する画面を選択可能
- **プリセット** — 好きな設定を保存・編集・適用
- **Super Duper Gaming Mode** — レインボー発光、速度、強度、Bloom幅を調整
- **ログイン時に起動** — Windowsのユーザー別Runエントリを使用
- **トレイアイコンを表示 / 非表示** — 隠してもRounderはバックグラウンドで動作を継続
- **Per-Monitor DPI対応** — 拡大率が異なる複数モニターでも角位置を合わせる
- **強い権限なし** — 通常ユーザー権限のローカルオーバーレイとして動作

## トレイからすぐ調整

トレイアイコンを表示しているときは、普段使う操作を設定画面を開かずに変更できます。Rounderのオン/オフ、角の半径、形状、クイック色、四隅の表示、Super Duper Gaming Mode、設定、終了をまとめています。

半径のスライダーや形状を変更すると、オーバーレイへその場で反映されます。トレイアイコンを非表示にした場合でも、`Rounder_Windows.exe` をもう一度起動すると既存プロセスの設定画面を開けます。二重起動はしません。

<p align="center">
  <img src="docs/assets/rounder-windows-tray-panel.png" alt="Rounder for Windows トレイ操作パネル" width="356" />
</p>

## ダウンロードとインストール

[Releases](https://github.com/nisesimadao/Rounder_Windows/releases/latest) から次のどちらかを使えます。

- **`Rounder_Windows_Setup.exe`** — 通常はこちら。インストールしてスタートメニューから起動します。
- **`Rounder_Windows.exe`** — self-contained の単一ファイル版。インストールせずそのまま実行できます。

Release版は .NET Runtime を同梱しているため、別途 .NET を入れる必要はありません。

現在のReleaseはコード署名していないため、初回起動時にWindows SmartScreenが警告する場合があります。GitHub Releasesから取得したファイルであることを確認してから実行してください。

## 初回起動

通常起動では設定画面が開きます。ログイン時の自動起動では設定画面を出さず、バックグラウンドで静かに起動します。

トレイアイコンを隠していてもRounder自体は動作し続けます。設定を再度開くときは `Rounder_Windows.exe` を起動してください。保存済みの「トレイアイコンを表示」設定は勝手に変更しません。

## 設定

設定画面はmacOS版Rounderと同じ構成に寄せています。

- **General** — Rounderの有効化、ログイン時起動、トレイアイコン表示
- **Appearance** — 半径、Rounded / Squircle / Polygon、色
- **Corners** — 四隅の個別オン/オフ
- **Displays** — 対象モニターの選択。すべてオフにすることも可能
- **Gaming** — レインボー発光、速度、強度、Bloom幅
- **Presets** — 保存、適用、編集、削除
- **About** — バージョン、技術情報、GitHub

<p align="center">
  <img src="docs/assets/rounder-windows-settings.png" alt="Rounder for Windows 設定画面" width="820" />
</p>

## システム要件

- Windows 10 または Windows 11
- x64 PC
- Release版: 追加ランタイム不要
- ソースからビルドする場合: .NET 9 SDK

## プライバシーと権限

Rounder for Windows は画面収録、アクセシビリティ、位置情報、マイク、カメラ、ネットワーク権限を要求しません。設定とプリセットは `%AppData%\Rounder` にJSONとして保存されます。

「ログイン時に起動」を有効にした場合のみ、`HKCU\Software\Microsoft\Windows\CurrentVersion\Run` にRounderの起動エントリを作成します。

詳しくは [Privacy](./docs/PRIVACY.md) と [Security](./docs/SECURITY.md) を参照してください。

## 技術メモ

- .NET 9 / `net9.0-windows`
- .NET 9 標準 Fluent Theme を使った WPF の設定画面・トレイクイック操作・プリセット編集・入力ダイアログ
- WinForms `ApplicationContext` / `NotifyIcon` は常駐と通知領域連携のみに使用
- クリック透過・topmost・per-pixel alpha の layered window
- GDI+ による Rounded / Squircle / Polygon 描画
- PerMonitorV2 DPI awareness
- `SystemEvents.DisplaySettingsChanged` でディスプレイ構成変更を検知
- 単一インスタンス + 手動再起動で既存設定画面を前面化

## ソースからビルド

```powershell
dotnet build .\Rounder_Windows.csproj -c Release
```

self-contained の単一EXE:

```powershell
dotnet publish .\Rounder_Windows.csproj `
  -c Release `
  -r win-x64 `
  --self-contained true `
  -p:PublishSingleFile=true `
  -p:EnableCompressionInSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -p:DebugType=None `
  -p:DebugSymbols=false `
  -o .\artifacts\release\Rounder_Windows-win-x64-singlefile
```

## リリース / CI

`main` / `master` へのpushでWindows runnerがビルドと配布物の検証を行います。`.csproj` の `<Version>` に対応する `v<Version>` Release がまだ存在しない場合は、自動でGitHub Releaseを作成します。

Releaseには次が添付されます。

- `Rounder_Windows.exe`
- `Rounder_Windows-win-x64-singlefile.zip`
- `Rounder_Windows_Setup.exe`

## プロジェクト文書

- [Changelog](./docs/CHANGELOG.md)
- [FAQ 日本語](./docs/FAQ.ja.md)
- [FAQ English](./docs/FAQ.md)
- [Privacy](./docs/PRIVACY.md)
- [Security](./docs/SECURITY.md)
- [Contributing](./docs/CONTRIBUTING.md)
- [License](./LICENSE)

## トラブルシューティング

**角丸が表示されない**
Rounderが有効になっているか、Displaysで対象モニターが選択されているか確認してください。全モニターをオフにすると意図的にオーバーレイは表示されません。

**変更が反映されない**
設定画面では **Apply** または **OK** を押してください。トレイメニューからの半径・形状・色・四隅・Gaming Mode変更はその場で反映されます。

**トレイアイコンを再表示したい**
`Rounder_Windows.exe` をもう一度起動してSettingsを開き、**Show tray icon** をオンにしてApplyしてください。

**フルスクリーンアプリの上に表示されない**
通常のボーダーレス / ウィンドウフルスクリーンではtopmostを維持しますが、セキュアデスクトップ、ロック画面、一部の排他フルスクリーンアプリの上には表示できません。
