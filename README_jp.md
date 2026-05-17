# Rounder for Windows

Rounder for Windows は、選択したモニターに角丸オーバーレイを描画する軽量な通知領域アプリです。

macOS 版 Rounder の挙動をできるだけ保ちながら、Windows では WinForms のトレイ常駐、クリック透過の最前面オーバーレイ、WPF の設定画面、JSON 設定、プリセット、マルチモニター選択、Super Duper Gaming Mode などで置き換えています。

![Rounder icon](Assets/rounder.png)

[English README](./README.md)

## 機能

- Windows の通知領域に常駐します。
- トレイアイコンの左クリックまたはダブルクリックで設定画面を開きます。
- トレイメニューから角丸オーバーレイの有効/無効を切り替えられます。
- オーバーレイを適用するモニターを選べます。
- 角丸半径、色、表示する角、ゲーミングモードのアニメーションを調整できます。
- プリセットを保存、編集、インポート、エクスポートできます。
- Fluent 風の WPF 設定画面を使い、ライト/ダークテーマに対応します。
- PerMonitorV2 DPI aware により、100% から 150% などの混在スケーリング環境に対応します。

## 必要環境

- Windows 10 または Windows 11
- .NET 8.0 Desktop Runtime
- ソースからビルドする場合は .NET 8.0 SDK

## ビルド

このディレクトリで実行します。

```powershell
dotnet build .\Rounder_Windows.csproj -c Release
```

実行ファイルは次の場所に出力されます。

```text
bin\Release\net8.0-windows\Rounder_Windows.exe
```

既に Rounder が起動している場合、Windows が exe をロックするためビルドに失敗します。トレイメニューから終了してから再ビルドしてください。

## 実行

```powershell
.\bin\Release\net8.0-windows\Rounder_Windows.exe
```

起動後、Rounder は通知領域に表示されます。トレイアイコンをクリックすると設定画面が開き、右クリックするとメニューが開きます。

## 設定

### General

- 角丸オーバーレイの有効/無効を切り替えます。
- モニター一覧を更新し、対象モニターを選択します。
- 角丸半径を 0 から 40 ピクセルで設定します。
- 標準色またはカラーピッカーで角の色を選択します。
- 4 つの角を個別に表示/非表示できます。

### Presets

- 保存済みプリセットを適用します。
- 現在の設定を新しいプリセットとして保存します。
- プリセットを編集または削除します。
- プリセットファイルをインポート/エクスポートします。

### Super Duper Gaming Mode

- 虹色アニメーションとグロー付きのオーバーレイを有効にします。
- アニメーション速度とグロー強度を調整できます。

## 実装メモ

- ターゲットフレームワーク: .NET 8, `net8.0-windows`
- トレイ常駐: Windows Forms `ApplicationContext` と `NotifyIcon`
- 設定画面: WPF と `iNKORE.UI.WPF.Modern`
- オーバーレイ描画: WinForms の最前面レイヤードウィンドウと GDI+
- クリック透過: `WS_EX_TRANSPARENT` などの Win32 拡張ウィンドウスタイル
- 設定保存: `%AppData%\Rounder` 配下の JSON
- DPI 対応: `HighDpiMode.PerMonitorV2`

## プロジェクト構成

```text
Rounder_Windows/
|-- Assets/                      アプリアイコンと画像アセット
|-- AppAssets.cs                  アイコン/画像の読み込み
|-- AppSettings.cs                設定モデル
|-- AppTheme.cs                   Windows テーマ検出ヘルパー
|-- CornerOverlayForm.cs          クリック透過オーバーレイウィンドウ
|-- CornerPreset.cs               プリセットモデル
|-- JsonStore.cs                  JSON 設定/プリセット保存
|-- OverlayManager.cs             オーバーレイのライフサイクル管理
|-- PresetEditorForm.cs           プリセット編集ダイアログ
|-- Program.cs                    STA エントリポイントと DPI 設定
|-- RounderApplicationContext.cs  トレイアイコンとアプリ寿命管理
|-- WpfSettingsWindow.xaml        Fluent 風の設定画面
|-- WpfThemeBootstrap.cs          WPF/iNKORE テーマ初期化
|-- Rounder_Windows.csproj        プロジェクトファイル
|-- README.md                     英語 README
`-- README_jp.md                  日本語 README
```

## トラブルシューティング

**角丸が表示されない**  
Rounder が有効になっているか、対象モニターが選択されているか確認してください。

**一部のアプリの上に表示されない**  
排他フルスクリーンアプリ、UAC などのセキュアデスクトップ、ロック画面は通常の最前面ウィンドウより上に表示されることがあります。

**ビルド時に `Rounder_Windows.exe` がロックされる**  
トレイメニューからアプリを終了するか、`Rounder_Windows` プロセスを停止してから再度ビルドしてください。
