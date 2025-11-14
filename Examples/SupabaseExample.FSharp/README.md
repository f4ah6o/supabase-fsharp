# Supabase F# Example

このプロジェクトは、Supabase.FSharpライブラリの包括的な使用例を提供します。F#特有の機能を活用したSupabaseの各種操作方法を実演します。

## 概要

このサンプルでは、以下のSupabase機能の使い方を示しています:

1. **クライアント作成と初期化** - Computation Expressionを使用した設定
2. **認証 (Authentication)** - サインアップ、サインアウト、ユーザー管理
3. **データベース操作** - PostgRESTを使用したCRUD操作
4. **リアルタイム (Realtime)** - WebSocketによるリアルタイム通信
5. **ストレージ (Storage)** - ファイルのアップロード・ダウンロード
6. **RPC (Remote Procedure Call)** - データベース関数の呼び出し
7. **Computation Expressions** - F#の計算式を使用したワークフロー
8. **パイプライン形式** - F#のパイプライン演算子を活用した操作

## 必要な環境

- .NET 9.0以降
- F# 9.0以降
- アクティブなSupabaseプロジェクト

## Supabase プロジェクトのセットアップ

このサンプルを実行する前に、Supabaseプロジェクトで以下の設定を行ってください。

### 1. Supabaseプロジェクトの作成

1. [Supabase](https://supabase.com/)にアクセスしてアカウントを作成
2. 新しいプロジェクトを作成
3. プロジェクトのURLとAPI Keyを取得
   - Dashboard > Settings > API から以下を確認:
     - `Project URL` (例: `https://xxxxx.supabase.co`)
     - `anon/public` key (匿名アクセス用のAPIキー)

### 2. データベーステーブルの作成

以下のSQLをSupabaseのSQL Editorで実行してください:

#### Movies テーブル

```sql
-- Movies テーブルの作成
CREATE TABLE movies (
    id BIGSERIAL PRIMARY KEY,
    name TEXT NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Row Level Security (RLS) の有効化
ALTER TABLE movies ENABLE ROW LEVEL SECURITY;

-- 匿名ユーザーに読み取り権限を付与
CREATE POLICY "Allow anonymous read access"
ON movies
FOR SELECT
TO anon
USING (true);

-- サンプルデータの挿入
INSERT INTO movies (name) VALUES
    ('The Matrix'),
    ('Inception'),
    ('Interstellar');
```

#### Users テーブル

```sql
-- Users テーブルの作成
CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    username TEXT UNIQUE NOT NULL,
    email TEXT UNIQUE NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Row Level Security (RLS) の有効化
ALTER TABLE users ENABLE ROW LEVEL SECURITY;

-- ユーザーは自分のデータのみ読み取り可能
CREATE POLICY "Users can read own data"
ON users
FOR SELECT
TO authenticated
USING (auth.uid() = id);

-- ユーザーは自分のデータのみ更新可能
CREATE POLICY "Users can update own data"
ON users
FOR UPDATE
TO authenticated
USING (auth.uid() = id);
```

### 3. Storageバケットの作成（オプション）

ストレージ機能を使用する場合:

1. Supabase Dashboard > Storage
2. 「Create a new bucket」をクリック
3. バケット名を `test-bucket` として作成
4. バケットのポリシー設定:
   - Public bucket: ON (パブリックアクセスを許可する場合)
   - または、適切なRow Level Securityポリシーを設定

### 4. RPC関数の作成（オプション）

RPC機能を使用する場合、以下のSQL関数を作成:

```sql
-- サンプルRPC関数
CREATE OR REPLACE FUNCTION hello_world(name TEXT)
RETURNS JSON AS $$
BEGIN
    RETURN json_build_object(
        'message', 'Hello, ' || name || '!',
        'timestamp', NOW()
    );
END;
$$ LANGUAGE plpgsql;

-- 匿名ユーザーに実行権限を付与
GRANT EXECUTE ON FUNCTION hello_world(TEXT) TO anon;
```

### 5. 認証設定

1. Supabase Dashboard > Authentication > Settings
2. 「Enable Email Confirmations」を無効化（開発環境の場合）
   - これにより、メール確認なしでユーザー登録が可能になります
3. 本番環境では、適切な認証設定とメール確認を有効化してください

### 6. Realtimeの有効化

1. Supabase Dashboard > Database > Replication
2. `movies` テーブルに対してRealtimeを有効化
   - テーブルを選択 > 「Enable Realtime」をON

## 環境変数の設定

以下の環境変数を設定してください:

```bash
export SUPABASE_URL="https://your-project.supabase.co"
export SUPABASE_KEY="your-anon-key"
```

### Windows (PowerShell)

```powershell
$env:SUPABASE_URL="https://your-project.supabase.co"
$env:SUPABASE_KEY="your-anon-key"
```

### Windows (コマンドプロンプト)

```cmd
set SUPABASE_URL=https://your-project.supabase.co
set SUPABASE_KEY=your-anon-key
```

### .envファイルを使用する場合

プロジェクトルートに `.env` ファイルを作成:

```env
SUPABASE_URL=https://your-project.supabase.co
SUPABASE_KEY=your-anon-key
```

注: `.env` ファイルを使用する場合は、実行前に環境変数を読み込む必要があります。

## プロジェクトのビルドと実行

### 1. 依存関係の復元

```bash
dotnet restore
```

### 2. プロジェクトのビルド

```bash
dotnet build
```

### 3. 実行

```bash
dotnet run
```

または、プロジェクトのルートディレクトリから:

```bash
dotnet run --project Examples/SupabaseExample.FSharp/SupabaseExample.FSharp.fsproj
```

## サンプルコードの説明

### Example 1: クライアントの作成と初期化

Computation Expressionを使用してSupabaseクライアントを設定し、初期化する方法を示します。

```fsharp
let options =
    supabaseOptions {
        schema "public"
        autoRefreshToken true
        autoConnectRealtime false
    }
```

**ポイント:**
- F#特有の`supabaseOptions` Computation Expressionを使用
- 環境変数から接続情報を取得
- F# Option型を使用した安全なNull処理

### Example 2: 認証 (Authentication)

ユーザーのサインアップ、ログイン、サインアウトの実装例です。

```fsharp
let! signUpResponse = Auth.signUp email password client
match Auth.currentUser client with
| Some user -> // ユーザーが存在する場合
| None -> // ユーザーが存在しない場合
```

**ポイント:**
- F# Async workflowsを使用
- Option型による安全なユーザー情報の取得
- `EmailOption` など、null許容型をOptionに変換

### Example 3: データベース操作

PostgRESTを使用したデータベースのクエリ操作です。

```fsharp
let moviesTable = Supabase.from<Movie> client
let! response = moviesTable.Get() |> Async.AwaitTask
```

**ポイント:**
- 型安全なモデル定義（`Movie` 型）
- TaskからF# Asyncへの変換
- LINQ風のクエリインターフェース

### Example 4: Realtime

WebSocketを使用したリアルタイム通信の実装です。

```fsharp
do! Realtime.connect client
let channel = Realtime.channel "public:movies" client
```

**ポイント:**
- F# Asyncを使用した接続管理
- チャンネルベースのサブスクリプション

### Example 5: Storage

ファイルのアップロード、ダウンロード、削除などのストレージ操作です。

```fsharp
let! uploadResult = Storage.upload bucketId filePath fileContent client
let publicUrl = Storage.publicUrl bucketId filePath client
```

**ポイント:**
- バイナリデータの扱い
- パブリックURLの生成
- ファイルのリスト取得

### Example 6: RPC (Remote Procedure Call)

PostgreSQLの関数をリモートから呼び出す方法です。

```fsharp
let parameters = {| name = "F# Developer" |}
let! result = Supabase.rpc procedureName parameters client
```

**ポイント:**
- 匿名レコードを使用したパラメータ渡し
- データベース関数の実行

### Example 7: Computation Expressions

F#の計算式を使用したワークフロー構築です。

```fsharp
let! result =
    auth {
        let! session = Auth.retrieveSession client
        // ワークフロー処理
        return Ok "Authenticated"
    }
```

**ポイント:**
- `auth` Computation Expression
- Result型を使用したエラーハンドリング
- F#らしい関数型プログラミング

### Example 8: パイプライン形式

F#のパイプライン演算子を活用した操作です。

```fsharp
let session =
    client
    |> Auth.currentSession
    |> Option.defaultWith (fun () -> null)
```

**ポイント:**
- パイプライン演算子 `|>` を使用
- 関数型スタイルのデータ変換
- Optionモジュールの関数との組み合わせ

## トラブルシューティング

### 認証エラー

```
Authentication error: Invalid credentials
```

**解決方法:**
- Supabaseプロジェクトで「Enable Email Confirmations」が無効化されているか確認
- メールアドレスとパスワードが要件を満たしているか確認（パスワードは最低6文字）

### データベース接続エラー

```
Database error: relation "movies" does not exist
```

**解決方法:**
- `movies` テーブルが作成されているか確認
- テーブル名が正確か確認（小文字・複数形）
- Row Level Security (RLS) ポリシーが正しく設定されているか確認

### ストレージエラー

```
Storage error: Bucket not found
```

**解決方法:**
- Supabase Dashboardで `test-bucket` が作成されているか確認
- バケットのアクセス権限設定を確認

### Realtimeエラー

```
Realtime error: Connection failed
```

**解決方法:**
- Supabaseプロジェクトでテーブルに対してRealtimeが有効化されているか確認
- WebSocket接続がファイアウォールでブロックされていないか確認

### 環境変数が設定されていない

```
SUPABASE_URL and SUPABASE_KEY environment variables must be set
```

**解決方法:**
- 環境変数が正しく設定されているか確認
- 同じターミナルセッションで環境変数を設定してから実行

## モデル定義

### Movie モデル

```fsharp
[<Table("movies")>]
type Movie() =
    inherit BaseModel()

    [<PrimaryKey("id")>]
    member val Id = 0 with get, set

    [<Column("name")>]
    member val Name = "" with get, set

    [<Column("created_at")>]
    member val CreatedAt = System.DateTime.MinValue with get, set
```

### UserProfile モデル

```fsharp
[<Table("users")>]
type UserProfile() =
    inherit BaseModel()

    [<PrimaryKey("id")>]
    member val Id = "" with get, set

    [<Column("username")>]
    member val Username = "" with get, set

    [<Column("email")>]
    member val Email = "" with get, set

    [<Column("created_at")>]
    member val CreatedAt = System.DateTime.MinValue with get, set
```

## 参考資料

- [Supabase公式ドキュメント](https://supabase.com/docs)
- [Supabase C# クライアント](https://github.com/supabase-community/supabase-csharp)
- [Supabase.FSharp ライブラリ](../../README.md)
- [F# ドキュメント](https://learn.microsoft.com/ja-jp/dotnet/fsharp/)

## ライセンス

このサンプルコードはMITライセンスの下で提供されています。

## 次のステップ

- より複雑な実例については [ContactApp](../ContactApp) を参照
- 本番環境での使用時は、適切な認証・認可設定を行ってください
- Row Level Security (RLS) ポリシーを本番環境に合わせて調整してください
