# PDF変換アプリケーション

![.NET Version](https://img.shields.io/badge/.NET-9.0-purple)
![Blazor](https://img.shields.io/badge/Blazor-WebAssembly-blue)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-9.0-green)
![License](https://img.shields.io/badge/license-MIT-blue.svg)

PDFファイルのアップロード、変換処理、ダウンロードを行うWebアプリケーションです。Blazor WebAssemblyとASP.NET Core APIで構築されています。

## 📸 スクリーンショット

![アプリケーション画面](docs/images/app-screenshot.png)

## 🌟 主な機能

- **PDFファイルアップロード**: ドラッグ&ドロップまたはファイル選択でPDFをアップロード
- **セキュアな変換処理**: ユーザーID、参照用・編集用パスワードを設定してPDF変換
- **自動ダウンロード**: 変換完了後、処理済みPDFを自動ダウンロード
- **リアルタイム状態表示**: API接続状態、処理進行状況を表示
- **レスポンシブデザイン**: モバイル・デスクトップ対応

## 🏗️ アーキテクチャ

```
┌─────────────────────┐    HTTPS/REST API    ┌─────────────────────┐
│                     │◄────────────────────►│                     │
│  Blazor WebAssembly │                      │  ASP.NET Core API   │
│  (フロントエンド)     │                      │  (バックエンド)       │
│                     │                      │                     │
└─────────────────────┘                      └─────────────────────┘
         │                                              │
         ▼                                              ▼
┌─────────────────────┐                      ┌─────────────────────┐
│   ブラウザ (SPA)     │                      │   PDF処理エンジン    │
│   ・ファイル選択      │                      │   ・入力値検証        │
│   ・入力フォーム      │                      │   ・ファイル変換      │
│   ・ダウンロード処理   │                      │   ・ログ出力         │
└─────────────────────┘                      └─────────────────────┘
```

## 🚀 クイックスタート

### 前提条件

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- HTTPS開発証明書の設定

### インストール・実行手順

1. **リポジトリのクローン**
   ```bash
   git clone https://github.com/yourusername/pdf-converter-app.git
   cd pdf-converter-app
   ```

2. **APIサーバーの起動**
   ```bash
   cd PdfConverterApi
   dotnet restore
   dotnet run
   # → https://localhost:7005 で起動
   ```

3. **Blazorアプリの起動** (新しいターミナル)
   ```bash
   cd PdfConverterApp
   dotnet restore
   dotnet run
   # → https://localhost:5001 で起動
   ```

4. **アプリケーションにアクセス**
   
   ブラウザで https://localhost:5001 を開く

### 動作確認

- API動作確認: `curl -X GET https://localhost:7005/api/data-convert`
- ブラウザでのアプリ動作確認

## 📁 プロジェクト構造

```
pdf-converter-app/
├── PdfConverterApi/              # ASP.NET Core API
│   ├── Controllers/
│   │   └── DataConvertController.cs
│   ├── Models/
│   │   └── ApiModels.cs
│   ├── Program.cs
│   └── appsettings.json
├── PdfConverterApp/              # Blazor WebAssembly
│   ├── Components/
│   │   └── Pages/
│   │       └── Home.razor
│   ├── Models/
│   │   └── ApiModels.cs
│   ├── Services/
│   │   └── PdfConverterService.cs
│   ├── Program.cs
│   └── wwwroot/
│       └── index.html
├── docs/                        # ドキュメント
├── README.md
└── .gitignore
```

## 🔧 設定・カスタマイズ

### APIエンドポイントの変更

`PdfConverterApp/Services/PdfConverterService.cs` の `_apiBaseUrl` を変更:

```csharp
private readonly string _apiBaseUrl = "https://your-api-domain.com/api/data-convert";
```

### ファイルサイズ制限の変更

**API側** (`PdfConverterApi/Controllers/DataConvertController.cs`):
```csharp
private const int MAX_FILE_SIZE = 20 * 1024 * 1024; // 20MB
```

**Blazor側** (`PdfConverterApp/Components/Pages/Home.razor`):
```csharp
private const int MAX_FILE_SIZE = 10 * 1024 * 1024; // 10MB
```

### CORS設定の調整

`PdfConverterApi/Program.cs` でBlazorアプリのURLを設定:

```csharp
policy.WithOrigins("https://localhost:5001", "http://localhost:5000")
```

## 🛡️ セキュリティ機能

- **入力値検証**: クライアント・サーバー両側での検証
- **ファイル形式チェック**: PDFヘッダーの検証
- **ファイルサイズ制限**: DoS攻撃対策
- **CORS制御**: 許可されたオリジンからのみアクセス可能
- **エラー情報制御**: 内部情報の漏洩防止

## 📊 API仕様

### POST /api/data-convert

PDFファイルの変換処理を実行します。

**リクエスト:**
```json
{
  "userId": "user001",
  "fileName": "document.pdf",
  "viewPassword": "view123",
  "editPassword": "edit456",
  "fileData": "base64encodeddata..."
}
```

**レスポンス (成功時):**
```json
{
  "fileName": "document_converted_20241216_143022.pdf",
  "fileData": "base64encodeddata..."
}
```

**レスポンス (エラー時):**
```json
{
  "message": "入力値が不正です",
  "details": "ファイルサイズが制限を超えています",
  "timestamp": "2024-12-16T14:30:22Z"
}
```

### GET /api/data-convert

API動作確認用エンドポイントです。

**レスポンス:**
```json
{
  "service": "PDF変換API",
  "version": "1.0.0",
  "status": "正常",
  "timestamp": "2024-12-16T14:30:22Z"
}
```

## 🧪 テスト

### 手動テスト手順

1. **正常系テスト**
   - 有効なPDFファイルをアップロード
   - 全必須項目を入力して送信
   - ダウンロードファイルの確認

2. **異常系テスト**
   - PDF以外のファイルをアップロード
   - 制限サイズ超過ファイルをアップロード
   - 必須項目を空のまま送信

### 自動テスト (将来実装予定)

```bash
# 単体テスト実行
dotnet test

# 統合テスト実行
dotnet test --filter Category=Integration
```

## 🚀 デプロイ

### Azure App Service

1. **API側デプロイ**
   ```bash
   cd PdfConverterApi
   dotnet publish -c Release
   # Azure App Serviceにデプロイ
   ```

2. **Blazor側デプロイ (Azure Static Web Apps)**
   ```bash
   cd PdfConverterApp
   dotnet publish -c Release
   # Azure Static Web Appsにデプロイ
   ```

### Docker (将来対応予定)

```dockerfile
# Dockerfile例
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
# ... 詳細設定
```

## 🤝 コントリビューション

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

## 📝 ライセンス

このプロジェクトは [MIT License](LICENSE) の下で公開されています。

## 👥 作者・貢献者

- **メイン開発者**: [@yourusername](https://github.com/yourusername)

## 📞 サポート・お問い合わせ

- **Issues**: [GitHub Issues](https://github.com/yourusername/pdf-converter-app/issues)
- **メール**: your.email@example.com
- **Twitter**: [@yourusername](https://twitter.com/yourusername)

## 🗺️ ロードマップ

### v1.1 (予定)
- [ ] 実際のPDF処理ライブラリ統合 (iTextSharp/PdfSharp)
- [ ] ユーザー認証システム
- [ ] 処理履歴機能

### v1.2 (予定)
- [ ] バッチ処理機能
- [ ] クラウドストレージ連携
- [ ] API利用統計ダッシュボード

### v2.0 (予定)
- [ ] Docker対応
- [ ] Kubernetes deployment
- [ ] マイクロサービス化

## 🙏 謝辞

- [Bootstrap](https://getbootstrap.com/) - UI フレームワーク
- [Bootstrap Icons](https://icons.getbootstrap.com/) - アイコンセット
- [Microsoft .NET Team](https://github.com/dotnet) - フレームワーク開発

---

⭐ このプロジェクトが役に立った場合は、スターをつけていただけると嬉しいです！