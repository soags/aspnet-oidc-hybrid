# ASP.NET OpenID Connect Hybric 認証

**検証用プロジェクトです。**

セッション認証などの既存の認証機能をもつASP.NET 系サーバーに対して、OpenID Connect 認証を後付けで対応する際の方法について検証しています。

## 準備

Docker コンテナ上でKeyCloak を実行します。

- [Keycloak Getting started (Docker)](https://www.keycloak.org/getting-started/getting-started-docker)

```shell
docker run -p 8080:8080 -e KEYCLOAK_ADMIN=admin -e KEYCLOAK_ADMIN_PASSWORD=admin quay.io/keycloak/keycloak:25.0.2 start-dev
```
