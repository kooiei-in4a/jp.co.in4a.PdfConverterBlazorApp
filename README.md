# PDF変換Webアプリケーション

Blazor WebAssemblyからASP.NET Core APIに通信するミニマムサンプルプロジェクトです。
PDFファイルのアップロード、変換処理、ダウンロードを行うWebアプリケーションです。Blazor WebAssemblyとASP.NET Core APIで構築されています。

## 機能

- **PDFファイルアップロード**: ドラッグ&ドロップまたはファイル選択でPDFをアップロード
- **セキュアな変換処理**: ユーザーID、参照用・編集用パスワードを設定してPDF変換
- **自動ダウンロード**: 変換完了後、処理済みPDFを自動ダウンロード
- **リアルタイム状態表示**: API接続状態、処理進行状況を表示
- **レスポンシブデザイン**: モバイル・デスクトップ対応

## システム構成

```
┌─────────────────────┐ HTTPS/REST API ┌─────────────────────┐
│                     │◄────────────────────►│                     │
│ Blazor WebAssembly  │                      │ ASP.NET Core API    │
│ (フロントエンド)     │                      │ (バックエンド)      │
│                     │                      │                     │
└─────────────────────┘                      └─────────────────────┘
            │                                           │
            ▼                                           ▼
┌─────────────────────┐                      ┌─────────────────────┐
│ ブラウザ (SPA)      │                      │ PDF処理エンジン     │
│ ・ファイル選択      │                      │ ・入力値検証        │
│ ・入力フォーム      │                      │ ・ファイル変換      │
│ ・ダウンロード処理  │                      │ ・ログ出力          │
└─────────────────────┘                      └─────────────────────┘
```

## 必要な環境

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- HTTPS開発証明書の設定

## 開発環境のセットアップ

### 1. リポジトリのクローン

```bash
git clone https://github.com/kooiei-in4a/jp.co.in4a.PdfConverterBlazorApp
cd jp.co.in4a.PdfConverterBlazorApp
```

### 2. APIサーバーの起動

```bash
cd PdfConverterApi
dotnet restore
dotnet run
# → http://localhost:5003/api/DataConvert で起動
```

### 3. Blazorアプリの起動 (新しいターミナル)

```bash
cd PdfConverterApp
dotnet restore
dotnet run
# → https://localhost:5001 で起動
```

### 4. アプリケーションにアクセス

ブラウザで [https://localhost:5001](https://localhost:5001) を開く

## API動作確認

- API動作確認:
  ```bash
  curl -X GET https://localhost:7005/api/data-convert
  ```
- ブラウザでのアプリ動作確認

## プロジェクト構造

```
pdf-converter-app/
├── PdfConverterApi/         # ASP.NET Core API
│   ├── Controllers/
│   │   └── DataConvertController.cs
│   ├── Program.cs
│   └── appsettings.json
├── PdfConverterApp/         # Blazor WebAssembly
│   ├── Components/
│   │   └── Pages/
│   │       └── Home.razor
│   ├── Services/
│   │   └── PdfConverterService.cs
│   ├── Program.cs
│   └── wwwroot/
│       └── index.html
├── PdfConverterShared/      # 共通Models
│   └── Models/
│       ├── ConvertRequest.cs
│       ├── ConvertResponse.cs
│       └── ErrorResponse.cs
├── docs/                    # ドキュメント
├── README.md
└── .gitignore
```

## 設定のカスタマイズ

### API接続先の変更

`PdfConverterApp/Services/PdfConverterService.cs` の `_apiBaseUrl` を変更:

```csharp
private readonly string _apiBaseUrl = "https://your-api-domain.com/api/data-convert";
```

### ファイルサイズ制限の変更

API側 (`PdfConverterApi/Controllers/DataConvertController.cs`):

```csharp
private const int MAX_FILE_SIZE = 20 * 1024 * 1024; // 20MB
```

Blazor側 (`PdfConverterApp/Components/Pages/Home.razor`):

```csharp
private const int MAX_FILE_SIZE = 10 * 1024 * 1024; // 10MB
```

### CORS設定

`PdfConverterApi/Program.cs` でBlazorアプリのURLを設定:

```csharp
policy.WithOrigins("https://localhost:5001", "http://localhost:5000")
```

## セキュリティ機能

- **入力値検証**: クライアント・サーバー両側での検証
- **ファイル形式チェック**: PDFヘッダーの検証
- **ファイルサイズ制限**: DoS攻撃対策
- **CORS制御**: 許可されたオリジンからのみアクセス可能
- **エラー情報制御**: 内部情報の漏洩防止

## API仕様

### POST /api/data-convert

PDFファイルの変換処理を実行します。

**リクエスト** (`PdfConverterShared.Models.ConvertRequest`):

```json
{
  "userId": "user001",
  "fileName": "document.pdf",
  "viewPassword": "view123",
  "editPassword": "edit456",
  "fileData": "base64encodeddata..."
}
```

**レスポンス** (成功時、`PdfConverterShared.Models.ConvertResponse`):

```json
{
  "fileName": "document_converted_20241216_143022.pdf",
  "fileData": "base64encodeddata..."
}
```

**レスポンス** (エラー時、`PdfConverterShared.Models.ErrorResponse`):

```json
{
  "message": "入力値が不正です",
  "details": "ファイルサイズが制限を超えています",
  "timestamp": "2024-12-16T14:30:22Z"
}
```

### GET /api/data-convert

API動作確認用エンドポイントです。

**レスポンス**:

```json
{
  "service": "PDF変換API",
  "version": "1.0.0",
  "status": "正常",
  "timestamp": "2024-12-16T14:30:22Z"
}
```

## テスト

### 手動テスト

#### 正常系テスト

- 有効なPDFファイルをアップロード
- 全必須項目を入力して送信
- ダウンロードファイルの確認

#### 異常系テスト

- PDF以外のファイルをアップロード
- 制限サイズ超過ファイルをアップロード
- 必須項目を空のまま送信

### 自動テスト

```bash
# 単体テスト実行
dotnet test

# 統合テスト実行
dotnet test --filter Category=Integration
```

## デプロイ

### API側デプロイ

```bash
cd PdfConverterApi
dotnet publish -c Release
# Azure App Serviceにデプロイ
```

### Blazor側デプロイ (Azure Static Web Apps)

```bash
cd PdfConverterApp
dotnet publish -c Release
# Azure Static Web Appsにデプロイ
```

### Docker対応

```dockerfile
# Dockerfile例
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# ... 詳細設定
```

## 貢献

1. このリポジトリをフォーク
2. 機能ブランチを作成 (`git checkout -b feature/amazing-feature`)
3. 変更をコミット (`git commit -m 'Add some amazing feature'`)
4. ブランチにプッシュ (`git push origin feature/amazing-feature`)
5. プルリクエストを作成

### 開発ガイドライン

- **コード規約**: Microsoft C# コーディング規約に準拠
- **コミットメッセージ**: Conventional Commits形式
- **テスト**: 新機能には必ずテストコードを追加
- **ドキュメント**: APIや設定変更時はドキュメントを更新

## ライセンス

このプロジェクトは [MIT License](LICENSE) の下で公開されています。

## サポート・お問い合わせ

- **メイン開発者**: [@yourusername](https://github.com/yourusername)
- **Issues**: [GitHub Issues](https://github.com/yourusername/pdf-converter-app/issues)
- **メール**: [your.email@example.com](mailto:your.email@example.com)
- **Twitter**: [@yourusername](https://twitter.com/yourusername)

## 今後の予定

- 実際のPDF処理ライブラリ統合 (iTextSharp/PdfSharp)
- ユーザー認証システム
- 処理履歴機能
- バッチ処理機能
- クラウドストレージ連携
- API利用統計ダッシュボード
- Docker対応
- Kubernetes deployment
- マイクロサービス化

## 使用技術

- [Bootstrap](https://getbootstrap.com/) - UI フレームワーク
- [Bootstrap Icons](https://icons.getbootstrap.com/) - アイコンセット

## 謝辞

- [Microsoft .NET Team](https://github.com/dotnet) - フレームワーク開発

---

⭐ このプロジェクトが役に立った場合は、スターをつけていただけると嬉しいです！