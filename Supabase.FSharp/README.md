# Supabase.FSharp

F#開発者向けのSupabaseクライアントの拡張機能とイディオマティックなラッパーです。

## インストール

```bash
dotnet add package Supabase
dotnet add package Supabase.FSharp
```

## 機能

### 1. F# Async ワークフロー対応

C#の`Task<T>`をF#の`Async<'T>`に自動変換:

```fsharp
open Supabase.FSharp

let! client = Supabase.initialize client
let! session = Auth.signIn "email@example.com" "password" client
```

### 2. Option型サポート

nullable値をF#の`Option<'T>`型として扱います:

```fsharp
match Auth.currentUser client with
| Some user -> printfn "User: %s" user.Email
| None -> printfn "Not logged in"
```

### 3. コンピュテーション式

宣言的な設定:

```fsharp
let options =
    supabaseOptions {
        schema "public"
        autoRefreshToken true
        autoConnectRealtime false
    }
```

### 4. 関数型スタイルのモジュール

パイプライン演算子を使った関数型スタイル:

```fsharp
let! movies =
    client
    |> Supabase.from<Movie>
    |> fun table -> table.Get()
    |> Async.AwaitTask
```

## 使用例

完全な例は[Documentation/FSharp.md](../Documentation/FSharp.md)および`Examples/SupabaseExample.FSharp`プロジェクトを参照してください。

### 基本的な使い方

```fsharp
open Supabase.FSharp

let main() = async {
    // クライアントを作成
    let options = supabaseOptions { autoRefreshToken true }
    let client = Supabase.create "YOUR_URL" "YOUR_KEY" options

    // 初期化
    let! client = Supabase.initialize client

    // 認証
    let! session = Auth.signIn "user@example.com" "password" client

    // データ取得
    let! movies =
        client
        |> Supabase.from<Movie>
        |> fun table -> table.Get()
        |> Async.AwaitTask

    return movies
}
```

## ドキュメント

詳細なドキュメントは[Documentation/FSharp.md](../Documentation/FSharp.md)をご覧ください。

## サポート

問題が発生した場合は、GitHubのIssueで報告してください:
https://github.com/supabase-community/supabase-csharp/issues
