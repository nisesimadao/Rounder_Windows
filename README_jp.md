# Rounder for Windows

Rounder for Windows は、選択したディスプレイの四隅にモダンな切り欠きオーバーレイを描画する、通知領域常駐アプリです。

この Windows 版は macOS 版 Rounder v2.1.4 の挙動にできるだけ寄せています。即時適用、モニター選択、プリセット、Rounded / Squircle / Polygon cutout、ログイン時起動、虹色のふち発光つき Super Duper Gaming Mode に対応しています。

![Rounder icon](Assets/rounder.png)

[English README](./README.md)

## 機能

- Windows の通知領域に常駐します。
- トレイアイコンの左クリックまたはダブルクリックで設定画面を開けます。
- トレイメニューから角丸効果をオン/オフできます。
- 設定変更はアプリ再起動なしで即時反映されます。
- 対象モニターを選択できます。新しく接続したモニターも自動的に対象になります。
- Rounded、Squircle、Polygon cutout の切り欠き形状を選べます。
- 半径、色、表示する角、ゲーミング速度、発光強度、Bloom 幅を調整できます。
- プリセットの保存、編集、インポート、エクスポートに対応しています。
- Windows の Run レジストリキーを使ったログイン時起動に対応しています。
- topmost z-order を再主張し、切り欠きウィンドウをタスクバーより上に維持します。
- PerMonitorV2 DPI aware により、混在スケーリング環境に対応します。

## 必要環境

- Windows 10 または Windows 11
- .NET 9.0 Desktop Runtime
- ソースからビルドする場合は .NET 9.0 SDK

## ビルド

```powershell
dotnet build .\Rounder_Windows.csproj -c Release
```

## 単一ファイル Release ビルド

```powershell
dotnet publish .\Rounder_Windows.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:DebugType=None -p:DebugSymbols=false -o .\artifacts\release\Rounder_Windows-win-x64-singlefile
```

出力される実行ファイル:

```text
artifacts\release\Rounder_Windows-win-x64-singlefile\Rounder_Windows.exe
```

## インストーラー作成

[Inno Setup 6](https://jrsoftware.org/isinfo.php) をインストールしてから実行します。

```powershell
& "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe" `
  "/DMyAppVersion=2.1.5" `
  "/DPublishDir=$PWD\artifacts\release\Rounder_Windows-win-x64-singlefile" `
  ".\installer\Rounder_Windows.iss"
```

出力されるインストーラー:

```text
artifacts\installer\Rounder_Windows_Setup.exe
```

## GitHub Actions リリース

`Build and Release` workflow は、push されたコミットを Windows runner 上でビルドします。

`main` または `master` への push では workflow artifact として成果物を保存します。さらに、`.csproj` の `<Version>` に対応する `v<Version>` リリースがまだ存在しない場合は、自動的に GitHub Release を作成し、以下をリリースに添付します（リリースノートは自動生成）。バージョンを上げて push するだけでリリースされます。

- `Rounder_Windows.exe`
- `Rounder_Windows-win-x64-singlefile.zip`
- `Rounder_Windows_Setup.exe`

`v*` タグを手動で push した場合や、workflow_dispatch でタグ名を指定した場合も、同じリリース処理が実行されます。

```powershell
git tag v2.1.5
git push origin v2.1.5
```

## 実装メモ

- ターゲットフレームワーク: .NET 9, `net9.0-windows`
- トレイ常駐: Windows Forms `ApplicationContext` と `NotifyIcon`
- 設定画面: iNKORE.UI.WPF.Modern のFluentスタイルとMica backdropを使ったWPF UI。macOS版のサイドバーと連続スクロールに寄せています
- オーバーレイ描画: クリック透過 topmost layered WinForms ウィンドウと per-pixel alpha
- ゲーミング発光: 辺ごとの透明な layered window による虹色グラデーション
- ログイン時起動: `HKCU\Software\Microsoft\Windows\CurrentVersion\Run`
- 設定保存: `%AppData%\Rounder` 配下の JSON

## トラブルシューティング

**角丸が表示されない**  
Rounder が有効になっているか、対象ディスプレイが選択されているか確認してください。

**システム UI の上に表示されない**  
Rounder は topmost z-order を再主張しますが、セキュアデスクトップ、ロック画面、一部の排他フルスクリーンアプリは通常アプリより上に表示されることがあります。

**ビルド時に `Rounder_Windows.exe` がロックされる**  
トレイメニューからアプリを終了するか、`Rounder_Windows` プロセスを停止してから再ビルドしてください。
