# QUY CHUẨN CHIẾN LƯỢC QUẢN LÝ NHÁNH GIT (ENTERPRISE GITFLOW WORKFLOW)

Tài liệu này định nghĩa quy chuẩn quản lý mã nguồn (Source Code Management - SCM) theo mô hình **Gitflow** dành cho môi trường doanh nghiệp, đảm bảo tính nhất quán, khả năng truy vết lịch sử (auditability), quy trình CI/CD tự động và độ ổn định cao nhất cho hệ thống Production.

---

## MỤC LỤC
1. [Kiến Trúc & Tổng Quan Luồng Phân Nhánh](#1-kiến-trúc--tổng-quan-luồng-phân-nhánh)
2. [Hệ Thống Phân Cấp & Vai Trò Các Nhánh](#2-hệ-thống-phân-cấp--vai-trò-các-nhánh)
3. [Quy Chuẩn Định Danh (Naming Conventions)](#3-quy-chuẩn-định-danh-naming-conventions)
4. [Quy Chuẩn Thông Điệp Commit (Conventional Commits)](#4-quy-chuẩn-thông-điệp-commit-conventional-commits)
5. [Quy Trình & Tập Lệnh CLI Chi Tiết (Native Git CLI)](#5-quy-trình--tập-lệnh-cli-chi-tiết-native-git-cli)
   - [5.1. Khởi Tạo Repository & Nhánh Cốt Lõi](#51-khởi-tạo-repository--nhánh-cốt-lõi)
   - [5.2. Luồng Phát Triển Tính Năng (Feature Workflow)](#52-luồng-phát-triển-tính-năng-feature-workflow)
   - [5.3. Luồng Đóng Gói Phát Hành (Release Workflow)](#53-luồng-đóng-gói-phát-hành-release-workflow)
   - [5.4. Luồng Xử Lý Sự Cố Khẩn Cấp (Hotfix Workflow)](#54-luồng-xử-lý-sự-cố-khẩn-cấp-hotfix-workflow)
6. [Tập Lệnh Nâng Cao Với `git-flow` Extension](#6-tập-lệnh-nâng-cao-với-git-flow-extension)
7. [Chính Sách Bảo Mật & Quản Trị Doanh Nghiệp (Governance)](#7-chính-sách-bảo-mật--quản-trị-doanh-nghiệp-governance)
8. [Bảng Tra Cứu Nhanh (Cheat Sheet)](#8-bảng-tra-cứu-nhanh-cheat-sheet)
9. [Quy Trình Làm Việc Thực Tế Với Dự Án ASP.NET Core & EF Core](#9-quy-trình-làm-việc-thực-tế-với-dự-án-aspnet-core--ef-core)
   - [9.1. Cấu Hình `.gitignore` Chuẩn Cho .NET / C#](#91-cấu-hình-gitignore-chuẩn-cho-net--c)
   - [9.2. Kịch Bản Thực Tế: Phát Triển Tính Năng CRUD Sản Phẩm](#92-kịch-bản-thực-tế-phát-triển-tính-năng-crud-sản-phẩm)
   - [9.3. Xử Lý Entity Framework Migrations Chuẩn Trong Git](#93-xử-lý-entity-framework-migrations-chuẩn-trong-git)
   - [9.4. Kiểm Tra Chất Lượng Bằng .NET CLI Trước Khi Commit](#94-kiểm-tra-chất-lượng-bằng-net-cli-trước-khi-commit)
   - [9.5. Quản Lý Cấu Hình & Bảo Mật Secrets Trong Git](#95-quản-lý-cấu-hình--bảo-mật-secrets-trong-git)
   - [9.6. Mẫu CI/CD Pipeline Tự Động Hóa Cho ASP.NET Core](#96-mẫu-cicd-pipeline-tự-động-hóa-cho-aspnet-core)

---

## 1. Kiến Trúc & Tổng Quan Luồng Phân Nhánh

Mô hình Gitflow phân tách vòng đời phần mềm thành hai nhóm nhánh chính:
- **Nhánh vô hạn (Infinite Lifespan / Long-lived branches)**: Tồn tại song song trong suốt vòng đời dự án (`master`/`main` và `develop`).
- **Nhánh tạm thời (Supporting / Short-lived branches)**: Được tạo ra cho từng mục đích cụ thể và bị xoá sau khi hoàn tất merge (`feature/*`, `release/*`, `hotfix/*`).

```mermaid
gitGraph
   commit id: "Init v0.1" tag: "Tag 0.1"
   branch develop
   checkout develop
   commit id: "Dev start"
   branch feature-1
   checkout feature-1
   commit id: "Feature 1 commit"
   checkout develop
   branch feature-2
   checkout feature-2
   commit id: "Feature 2 commit"
   checkout develop
   merge feature-1 id: "Merge feature-1"
   
   checkout master
   branch hotfix-0.2
   checkout hotfix-0.2
   commit id: "Fix bug prod"
   checkout master
   merge hotfix-0.2 id: "Merge hotfix" tag: "Tag 0.2"
   checkout develop
   merge hotfix-0.2 id: "Sync hotfix to develop"
   
   checkout develop
   branch release-1.0
   checkout release-1.0
   commit id: "Bump version 1.0"
   commit id: "Fix release bug"
   checkout develop
   merge release-1.0 id: "Sync bugfix to dev"
   
   checkout master
   merge release-1.0 id: "Release 1.0" tag: "Tag 1.0"
   checkout develop
   merge release-1.0 id: "Merge back release 1.0"
```

---

## 2. Hệ Thống Phân Cấp & Vai Trò Các Nhánh

| Tên Nhánh | Nhánh Gốc (Branch From) | Merge Về (Merge Into) | Môi Trường Triển Khai | Mục Đích & Ràng Buộc |
| :--- | :--- | :--- | :--- | :--- |
| **`master`** (hoặc `main`) | *Không* | *Không* | **Production** | Chứa mã nguồn ổn định nhất đang chạy thực tế. **Tuyệt đối cấm commit trực tiếp**. Mọi thay đổi phải đi qua PR từ `release/*` hoặc `hotfix/*` và phải luôn được đánh Tag phiên bản. |
| **`develop`** | `master` | *Không* | **Staging / SIT** | Nhánh tích hợp mã nguồn chính của các lập trình viên. Đóng vai trò phản ánh các tính năng sẽ được phát hành trong kỳ release tiếp theo. |
| **`feature/*`** | `develop` | `develop` | **Local / Dev Test** | Dùng để phát triển tính năng hoặc tác vụ độc lập. Sau khi hoàn thành và vượt qua Code Review/CI, merge về `develop` với flag `--no-ff`. |
| **`release/*`** | `develop` | `master` **&** `develop` | **UAT / Pre-Prod** | Đóng gói phiên bản sắp phát hành. **Chỉ chấp nhận bugfix, hoàn thiện tài liệu, cập nhật version/metadata**, không thêm feature mới. Khi xong, merge về `master` (đánh Tag) và merge ngược về `develop`. |
| **`hotfix/*`** | `master` | `master` **&** `develop` | **Staging -> Prod** | Sửa lỗi nghiêm trọng phát sinh trên Production. Sau khi vá xong, merge về `master` (đánh Tag vá lỗi) và merge ngược về `develop` (hoặc nhánh `release/*` đang mở). |

---

## 3. Quy Chuẩn Định Danh (Naming Conventions)

Tất cả các thành viên trong đội ngũ kỹ thuật bắt buộc tuân thủ tiền tố định danh kết hợp mã định danh công việc (Jira/Azure DevOps ticket ID):

```
<loại-nhánh>/<mã-ticket>-<mô-tả-ngắn-gọn>
```

- **Feature**:
  - Cú pháp: `feature/<ticket-id>-<short-description>`
  - Ví dụ: `feature/PROJ-102-integrate-vnpay-gateway`, `feature/AUTH-45-oauth2-login`
- **Bugfix (Dành cho việc fix bug trên develop)**:
  - Cú pháp: `bugfix/<ticket-id>-<short-description>`
  - Ví dụ: `bugfix/PROJ-140-fix-null-pointer-cart`
- **Release**:
  - Cú pháp: `release/v<Major>.<Minor>.<Patch>`
  - Ví dụ: `release/v1.0.0`, `release/v2.1.0`
- **Hotfix**:
  - Cú pháp: `hotfix/v<Major>.<Minor>.<Patch>` hoặc `hotfix/<ticket-id>-<short-description>`
  - Ví dụ: `hotfix/v1.0.1`, `hotfix/SEC-99-sql-injection-patch`
- **Release Tags**:
  - Cú pháp: `v<Major>.<Minor>.<Patch>` (Tuân thủ Semantic Versioning 2.0.0)
  - Ví dụ: `v0.1.0`, `v0.2.0`, `v1.0.0`

---

## 4. Quy Chuẩn Thông Điệp Commit (Conventional Commits)

Áp dụng chuẩn quốc tế [Conventional Commits v1.0.0](https://www.conventionalcommits.org/) để phục vụ sinh tự động changelog và đánh version:

```
<type>(<scope>): <subject>

[optional body]

[optional footer(s)]
```

### Các Loại Type Chuẩn:
- `feat`: Tính năng mới cho người dùng.
- `fix`: Sửa lỗi cho hệ thống.
- `refactor`: Tái cấu trúc mã nguồn không làm thay đổi tính năng hiện có.
- `perf`: Cải thiện hiệu năng.
- `docs`: Bổ sung, cập nhật tài liệu.
- `test`: Thêm hoặc sửa đổi unit/integration test.
- `chore`: Tác vụ bảo trì, cấu hình build pipeline, cập nhật dependency.

### Ví Dụ:
```git
feat(payment): add VNPay QR code checkout handler

Implement IPN listener and cryptographic signature verification
according to VNPay API v2.1 specifications.

Closes: #PROJ-102
Reviewed-by: @lead-dev
```

---

## 5. Quy Trình & Tập Lệnh CLI Chi Tiết (Native Git CLI)

### 5.1. Khởi Tạo Repository & Nhánh Cốt Lõi

Khi bắt đầu dự án mới, thiết lập nhánh `master` và phân tách nhánh `develop`.

```bash
# 1. Khởi tạo repository cục bộ
git init
git checkout -b master

# 2. Tạo commit ban đầu
echo "# Enterprise Application" > README.md
git add README.md
git commit -m "chore: initial project structure"

# 3. Gắn Tag phiên bản khởi tạo
git tag -a v0.1.0 -m "Release version 0.1.0"

# 4. Phân nhánh develop từ master
git checkout -b develop

# 5. Liên kết Remote Repository và đẩy các nhánh lên Server
git remote add origin https://github.com/company/enterprise-core.git
git push -u origin master --tags
git push -u origin develop
```

---

### 5.2. Luồng Phát Triển Tính Năng (Feature Workflow)

Áp dụng khi lập trình viên bắt đầu nhận một nhiệm vụ mới từ backlog.

#### Bước 1: Tạo nhánh Feature từ nhánh `develop` mới nhất
```bash
# Đảm bảo đang ở develop và đã cập nhật mã nguồn mới nhất
git checkout develop
git pull origin develop

# Tạo và chuyển sang nhánh feature mới
git checkout -b feature/PROJ-101-user-authentication
```

#### Bước 2: Phát triển và Commit mã nguồn định kỳ
```bash
# Thêm file thay đổi vào staging area
git add src/Services/AuthService.cs tests/AuthServiceTests.cs

# Commit tuân thủ chuẩn Conventional Commits
git commit -m "feat(auth): implement JWT token generation and validation"
```

#### Bước 3: Đồng bộ liên tục với nhánh `develop` (Tránh xung đột lớn)
Trước khi mở PR, cần rebase hoặc merge develop mới nhất vào feature:
```bash
git fetch origin
git rebase origin/develop
# Nếu có xung đột (conflict): Sửa xung đột -> git add <file> -> git rebase --continue
```

#### Bước 4: Đẩy nhánh lên Remote Repository
```bash
git push -u origin feature/PROJ-101-user-authentication
```

#### Bước 5: Mở Pull Request (PR / MR) và Hoàn tất (Merge)
- **Quy tắc doanh nghiệp**: Bắt buộc tạo Pull Request trên giao diện Web (GitHub/GitLab/Bitbucket), chạy tự động CI/CD Pipeline (Build + Test + SonarQube scan) và yêu cầu tối thiểu **1-2 Senior/Lead duyệt (Approval)**.
- Khi hoàn tất, thực hiện merge với cờ `--no-ff` (No Fast-Forward) để bảo toàn lịch sử nhánh:

```bash
# (Thực hiện trên máy hoặc qua Web UI merge button)
git checkout develop
git pull origin develop
git merge --no-ff feature/PROJ-101-user-authentication -m "merge: pull request #12 from feature/PROJ-101-user-authentication into develop"

# Đẩy nhánh develop đã cập nhật lên server
git push origin develop

# Xoá nhánh feature cục bộ và từ xa để dọn dẹp
git branch -d feature/PROJ-101-user-authentication
git push origin --delete feature/PROJ-101-user-authentication
```

---

### 5.3. Luồng Đóng Gói Phát Hành (Release Workflow)

Áp dụng khi các tính năng cho đợt phát hành đã sẵn sàng trên `develop`. Tại thời điểm này, nhánh `release` được tạo ra để phục vụ kiểm thử chấp nhận người dùng (UAT), đóng gói phiên bản, cập nhật số version và tài liệu.

#### Bước 1: Phân nhánh Release từ `develop`
```bash
git checkout develop
git pull origin develop

# Tạo nhánh release cho phiên bản v1.0.0
git checkout -b release/v1.0.0

# Đẩy nhánh release lên remote để hệ thống CI triển khai lên môi trường UAT/Staging
git push -u origin release/v1.0.0
```

#### Bước 2: Cập nhật Metadata, Bump Version và Sửa lỗi nhỏ
> **Lưu ý quan trọng**: Không được thêm feature mới vào nhánh này! Chỉ fix bug được phát hiện trong quá trình UAT.

```bash
# Cập nhật số phiên bản trong file cấu hình (ví dụ: package.json, pom.xml, csproj)
git commit -am "chore(release): bump version to 1.0.0"

# Fix lỗi phát sinh trong quá trình kiểm thử
git commit -am "fix(checkout): resolve rounding calculation issue on tax invoice"
git push origin release/v1.0.0
```

#### Bước 3: Đồng bộ bugfix từ `release` ngược lại vào `develop` (Liên tục nếu cần)
Nếu quá trình test kéo dài nhiều ngày, các bản fix trên release nên được đồng bộ về `develop` để tránh lệch code:
```bash
git checkout develop
git pull origin develop
git merge --no-ff release/v1.0.0 -m "sync: merge bugfixes from release/v1.0.0 back to develop"
git push origin develop
```

#### Bước 4: Hoàn tất Đóng Gói và Phát Hành (Finish Release)
Khi phiên bản đã được Product Owner và QA phê duyệt:

1. **Merge vào `master` và gắn Tag phát hành:**
```bash
# Chuyển sang master và cập nhật
git checkout master
git pull origin master

# Merge nhánh release vào master (Bắt buộc --no-ff)
git merge --no-ff release/v1.0.0 -m "release: merge release/v1.0.0 into master for Production deployment"

# Gắn Tag phiên bản có chữ ký hoặc chú thích
git tag -a v1.0.0 -m "Release production version 1.0.0"

# Đẩy lên master và tags
git push origin master --tags
```

2. **Merge ngược lại vào `develop` để bảo toàn toàn bộ bản sửa lỗi:**
```bash
git checkout develop
git pull origin develop
git merge --no-ff release/v1.0.0 -m "merge: release/v1.0.0 back into develop"
git push origin develop
```

3. **Dọn dẹp nhánh Release:**
```bash
git branch -d release/v1.0.0
git push origin --delete release/v1.0.0
```

---

### 5.4. Luồng Xử Lý Sự Cố Khẩn Cấp (Hotfix Workflow)

Áp dụng khi phát hiện sự cố nghiêm trọng (Critical Bug / Security Vulnerability) trên môi trường Production đang chạy cần khắc phục ngay lập tức mà không thể đợi đợt release tiếp theo.

#### Bước 1: Tạo nhánh Hotfix trực tiếp từ `master`
```bash
git checkout master
git pull origin master

# Tạo nhánh hotfix mới từ master (hoặc từ tag bị lỗi)
git checkout -b hotfix/v1.0.1
```

#### Bước 2: Khắc phục lỗi khẩn cấp và kiểm thử
```bash
# Sửa mã nguồn và commit
git commit -am "fix(security): resolve SQL injection vulnerability in search query"

# Tăng version bản vá
git commit -am "chore(version): bump version to 1.0.1"
```

#### Bước 3: Hoàn tất Hotfix (Merge đồng thời vào `master` và `develop`)

1. **Đưa bản vá vào `master` và đánh Tag bản vá mới:**
```bash
git checkout master
git pull origin master

# Merge hotfix vào master
git merge --no-ff hotfix/v1.0.1 -m "hotfix: merge hotfix/v1.0.1 into master"

# Gắn Tag phiên bản hotfix
git tag -a v1.0.1 -m "Hotfix version 1.0.1 - Fix critical security vulnerability"

# Đẩy lên remote
git push origin master --tags
```

2. **Đồng bộ bản vá ngược lại vào `develop`:**
```bash
git checkout develop
git pull origin develop

# Merge hotfix vào develop để tránh tái phát sinh lỗi trong tương lai
git merge --no-ff hotfix/v1.0.1 -m "hotfix: sync hotfix/v1.0.1 changes into develop"
git push origin develop
```

> **Trường hợp đặc biệt**: Nếu tại thời điểm đó đang có một nhánh `release/*` tồn tại, bản hotfix phải được merge vào nhánh `release/*` đó thay vì `develop`. Khi nhánh `release/*` kết thúc, các thay đổi sẽ tự động được đưa về `develop`.

3. **Xoá nhánh Hotfix:**
```bash
git branch -d hotfix/v1.0.1
git push origin --delete hotfix/v1.0.1
```

---

## 6. Tập Lệnh Nâng Cao Với `git-flow` Extension

Để tự động hóa các thao tác merge 2 nhánh cùng lúc và gắn tag chuẩn xác, doanh nghiệp có thể chuẩn hóa việc sử dụng công cụ CLI mở rộng `git-flow` (hoặc `git-flow-avh`):

### 6.1. Cài Đặt
- **macOS**: `brew install git-flow-avh`
- **Ubuntu/Debian**: `sudo apt-get install git-flow`
- **Windows**: Đã tích hợp sẵn trong Git for Windows.

### 6.2. Khởi Tạo Dự Án
```bash
git flow init -d
# Lệnh này sẽ tự động thiết lập các tiền tố nhánh:
# - Production branch: master
# - Development branch: develop
# - Feature prefix: feature/
# - Bugfix prefix: bugfix/
# - Release prefix: release/
# - Hotfix prefix: hotfix/
# - Version tag prefix: v
```

### 6.3. Bảng Lệnh `git-flow` Rút Gọn

| Tác Vụ | Lệnh Bắt Đầu (Start) | Lệnh Đẩy/Xuất Bản (Publish) | Lệnh Kết Thúc (Finish) |
| :--- | :--- | :--- | :--- |
| **Feature** | `git flow feature start PROJ-101-auth` | `git flow feature publish PROJ-101-auth` | `git flow feature finish PROJ-101-auth` |
| **Release** | `git flow release start 1.0.0` | `git flow release publish 1.0.0` | `git flow release finish 1.0.0` |
| **Hotfix** | `git flow hotfix start 1.0.1` | `git flow hotfix publish 1.0.1` | `git flow hotfix finish 1.0.1` |

*Khi chạy `git flow release finish` hoặc `git flow hotfix finish`, công cụ sẽ tự động:*
1. Merge vào `master`.
2. Gắn Git Tag theo tên phiên bản.
3. Merge ngược lại vào `develop`.
4. Xoá nhánh local vừa hoàn thành.

---

## 7. Chính Sách Bảo Mật & Quản Trị Doanh Nghiệp (Governance)

### 7.1. Cấu Hình Khóa Nhánh (Branch Protection Rules)
Cấu hình bắt buộc trên GitHub / GitLab / Azure Repos:

1. **Bảo vệ nhánh `master` & `develop`**:
   - `Require a pull request before merging`: Bắt buộc tạo PR, cấm push trực tiếp (`git push --force` bị vô hiệu hóa).
   - `Require approvals`: Tối thiểu 1 Senior Developer hoặc Tech Lead phê duyệt.
   - `Require status checks to pass before merging`: CI Pipeline phải vượt qua (Unit Tests, Lint, SonarQube Quality Gate).
   - `Require signed commits`: Bắt buộc sử dụng khóa GPG để ký commit, xác minh danh tính lập trình viên.
   - `Include administrators`: Áp dụng quy tắc cho cả quản trị viên repository.

### 7.2. Tiêu Chí Nghiệm Thu Pull Request (PR Checklist)
Trước khi phê duyệt bất kỳ PR nào vào `develop` hoặc `master`, reviewer cần kiểm tra:
- [ ] Mã nguồn tuân thủ coding standards và linting rules của dự án.
- [ ] Đã bổ sung Unit Tests cho tính năng mới với Code Coverage đạt tối thiểu 80%.
- [ ] Không chứa thông tin nhạy cảm (API Keys, Passwords, Connection Strings) - đã qua kiểm tra của GitGuardian/TruffleHog.
- [ ] Lịch sử commit rõ ràng, không chứa các commit tạm (`fix typo`, `test`, `wip`).

### 7.3. Chiến Lược Merge Mã Nguồn (Merge Strategy)
- **Từ Feature -> Develop**: Sử dụng **`--no-ff` (Create a merge commit)** hoặc **Squash and Merge** (nếu feature có quá nhiều commit nhỏ vụn vặt) để giữ lịch sử trực quan.
- **Từ Release/Hotfix -> Master**: Bắt buộc sử dụng **`--no-ff`** để bảo toàn điểm chia tách và điểm hợp nhất trên biểu đồ git.

---

## 8. Bảng Tra Cứu Nhanh (Cheat Sheet)

```bash
# ==========================================
# 1. HÀNG NGÀY: PHÁT TRIỂN FEATURE
# ==========================================
git checkout develop && git pull
git checkout -b feature/<id>-<ten>
# ... làm việc và commit ...
git commit -m "feat(<scope>): <nội dung>"
git push -u origin feature/<id>-<ten>
# Tạo Pull Request trên Web UI

# ==========================================
# 2. ĐỊNH KỲ: ĐÓNG GÓI BẢN PHÁT HÀNH (RELEASE)
# ==========================================
git checkout develop && git pull
git checkout -b release/v1.0.0
# ... sửa lỗi nhẹ, cập nhật version ...
git commit -am "chore(release): bump version to 1.0.0"
# Merge vào master:
git checkout master && git pull
git merge --no-ff release/v1.0.0
git tag -a v1.0.0 -m "Release version 1.0.0"
git push origin master --tags
# Merge ngược lại vào develop:
git checkout develop && git pull
git merge --no-ff release/v1.0.0
git push origin develop
# Xoá nhánh release:
git branch -d release/v1.0.0

# ==========================================
# 3. KHẨN CẤP: SỬA LỖI PRODUCTION (HOTFIX)
# ==========================================
git checkout master && git pull
git checkout -b hotfix/v1.0.1
# ... sửa lỗi nghiêm trọng ...
git commit -am "fix(prod): sửa lỗi bảo mật khẩn cấp"
# Đưa vào master:
git checkout master && git pull
git merge --no-ff hotfix/v1.0.1
git tag -a v1.0.1 -m "Hotfix version 1.0.1"
git push origin master --tags
# Đưa ngược lại vào develop:
git checkout develop && git pull
git merge --no-ff hotfix/v1.0.1
git push origin develop
# Xoá nhánh hotfix:
git branch -d hotfix/v1.0.1
```

---

## 9. Quy Trình Làm Việc Thực Tế Với Dự Án ASP.NET Core & EF Core

Phần này hướng dẫn toàn diện quy trình áp dụng Gitflow khi phát triển hệ thống backend sử dụng **ASP.NET Core Web API** và **Entity Framework Core (EF Core)** theo tiêu chuẩn doanh nghiệp.

### 9.1. Cấu Hình `.gitignore` Chuẩn Cho .NET / C#

Trước khi đẩy bất kỳ mã nguồn .NET nào lên Git, repository bắt buộc phải có file `.gitignore` tiêu chuẩn để ngăn commit các thư mục build tạm thời (`bin/`, `obj/`), cấu hình cá nhân của IDE và các file chứa secret.

```bash
# Tạo file .gitignore chuẩn cho .NET bằng dotnet CLI:
dotnet new gitignore
```

Các quy tắc quan trọng bắt buộc có trong `.gitignore`:
```gitignore
# Thư mục build và output
[Bb]in/
[Oo]bj/
publish/

# IDE & Editor settings
.vs/
.vscode/
.idea/
*.user
*.suo

# Cấu hình chứa secrets nhạy cảm (CẤM COMMIT LÊN GIT)
appsettings.Development.json
appsettings.Local.json
*.secrets.json

# Crash dumps & test results
TestResults/
*.crashdump
```

---

### 9.2. Kịch Bản Thực Tế: Phát Triển Tính Năng CRUD Sản Phẩm

Giả sử đội ngũ nhận User Story từ Jira: **`PROJ-105: Phát triển cụm API CRUD Quản lý Sản phẩm (Product Management)`**.

#### Bước 1: Phân nhánh từ `develop` mới nhất
```bash
git checkout develop
git pull origin develop

# Tạo nhánh tính năng theo đúng quy chuẩn định danh
git checkout -b feature/PROJ-105-product-crud
```

#### Bước 2: Xây dựng cấu trúc mã nguồn ASP.NET Core

Triển khai mã nguồn tuân thủ kiến trúc phân tầng sạch (Clean Architecture / N-Tier Architecture):

1. **Entity Database (`Models/Product.cs`)**:
```csharp
namespace MyEnterpriseApp.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
```

2. **DTOs (`DTOs/ProductDtos.cs`)**:
```csharp
using System.ComponentModel.DataAnnotations;

namespace MyEnterpriseApp.DTOs;

public class CreateProductDto
{
    [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    [Range(0.01, double.MaxValue, ErrorMessage = "Giá phải lớn hơn 0")]
    public decimal Price { get; set; }
    [Range(0, int.MaxValue)]
    public int Stock { get; set; }
}

public class UpdateProductDto : CreateProductDto {}

public class ProductResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

3. **Database Context (`Data/AppDbContext.cs`)**:
```csharp
using Microsoft.EntityFrameworkCore;
using MyEnterpriseApp.Models;

namespace MyEnterpriseApp.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Price).HasPrecision(18, 2);
        });
    }
}
```

4. **RESTful Controller (`Controllers/ProductsController.cs`)**:
```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyEnterpriseApp.Data;
using MyEnterpriseApp.DTOs;
using MyEnterpriseApp.Models;

namespace MyEnterpriseApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProductsController(AppDbContext context)
    {
        _context = context;
    }

    // 1. READ ALL (Phân trang)
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var data = await _context.Products.AsNoTracking()
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductResponseDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Stock = p.Stock,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();

        return Ok(data);
    }

    // 2. READ BY ID
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        if (product == null) return NotFound(new { message = $"Không tìm thấy sản phẩm #{id}" });

        return Ok(new ProductResponseDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            CreatedAt = product.CreatedAt
        });
    }

    // 3. CREATE
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
    {
        var entity = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            Stock = dto.Stock,
            CreatedAt = DateTime.UtcNow
        };

        _context.Products.Add(entity);
        await _context.SaveChangesAsync();

        var res = new ProductResponseDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            Price = entity.Price,
            Stock = entity.Stock,
            CreatedAt = entity.CreatedAt
        };

        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, res);
    }

    // 4. UPDATE
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dto)
    {
        var entity = await _context.Products.FindAsync(id);
        if (entity == null) return NotFound(new { message = $"Không tìm thấy sản phẩm #{id}" });

        entity.Name = dto.Name;
        entity.Description = dto.Description;
        entity.Price = dto.Price;
        entity.Stock = dto.Stock;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // 5. DELETE
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _context.Products.FindAsync(id);
        if (entity == null) return NotFound(new { message = $"Không tìm thấy sản phẩm #{id}" });

        _context.Products.Remove(entity);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
```

---

### 9.3. Xử Lý Entity Framework Migrations Chuẩn Trong Git

Xung đột Migration là vấn đề rất thường gặp trong môi trường doanh nghiệp khi nhiều lập trình viên cùng tạo migration trên các nhánh feature khác nhau.

#### Quy tắc quản lý Migration qua Git:
1. **Tạo Migration với tên rõ ràng theo convention**:
   ```bash
   dotnet ef migrations add AddProductTable --project MyEnterpriseApp
   ```
2. **Cập nhật database cục bộ (Local Developer DB)**:
   ```bash
   dotnet ef database update --project MyEnterpriseApp
   ```
3. **Các file sinh ra bởi EF Core PHẢI được commit lên Git**:
   - `Migrations/<Timestamp>_AddProductTable.cs`
   - `Migrations/<Timestamp>_AddProductTable.Designer.cs`
   - `Migrations/AppDbContextModelSnapshot.cs` (Đồng bộ snapshot toàn dự án)

#### Xử lý xung đột Migration Snapshot khi Rebase `develop`:
Nếu đồng nghiệp đã merge một migration khác vào `develop` trước bạn:
1. Rebase nhánh của bạn lên `develop`:
   ```bash
   git fetch origin
   git rebase origin/develop
   ```
2. Nếu gặp xung đột trong file `AppDbContextModelSnapshot.cs`:
   - Gỡ bỏ migration cục bộ của bạn: `dotnet ef migrations remove`
   - Đồng bộ database về trạng thái của `develop`: `dotnet ef database update`
   - Tạo lại migration mới sau khi code base đã cập nhật: `dotnet ef migrations add AddProductTable`
   - Kiểm tra và tiếp tục rebase.

> **Cảnh báo quan trọng**: Tuyệt đối không gọi `context.Database.Migrate()` trong `Program.cs` khi khởi chạy ứng dụng Production. Thay vào đó, trong pipeline CI/CD, hãy sinh script SQL độc lập để DBA hoặc hệ thống release tự động chạy:
> ```bash
> dotnet ef migrations script --idempotent --output ./publish/migration.sql
> ```

---

### 9.4. Kiểm Tra Chất Lượng Bằng .NET CLI Trước Khi Commit

Lập trình viên bắt buộc chạy các lệnh kiểm thử và định dạng mã nguồn cục bộ trước khi thực hiện commit:

```bash
# 1. Định dạng mã nguồn theo tiêu chuẩn .editorconfig
dotnet format --verify-no-changes

# 2. Build dự án ở chế độ Release (Kiểm tra cảnh báo và lỗi biên dịch)
dotnet build --configuration Release --no-incremental

# 3. Chạy toàn bộ Unit Tests & Integration Tests
dotnet test --configuration Release --logger "console;verbosity=normal"
```

Khi mọi kiểm tra đã đạt (Green Build), thực hiện commit và đẩy lên nhánh:
```bash
# Staging các thay đổi
git add Models/ DTOs/ Data/ Controllers/ Migrations/

# Commit với mã ticket Jira
git commit -m "feat(product): implement full CRUD APIs with EF Core and migration"

# Rebase nhẹ nhàng với develop trước khi push
git fetch origin
git rebase origin/develop

# Push nhánh feature lên server
git push -u origin feature/PROJ-105-product-crud
```

---

### 9.5. Quản Lý Cấu Hình & Bảo Mật Secrets Trong Git

Doanh nghiệp tuyệt đối cấm commit các thông tin mật (Connection Strings, JWT Secret Key, API Keys) vào bất kỳ nhánh Git nào.

- **Môi trường phát triển cục bộ (Local Development)**:
  Sử dụng công cụ `dotnet user-secrets` tích hợp sẵn của .NET để lưu trữ mật khẩu bên ngoài thư mục code của Git:
  ```bash
  # Khởi tạo user secrets cho project
  dotnet user-secrets init

  # Lưu connection string cục bộ (không lưu vào appsettings.json)
  dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=DevDb;User Id=sa;Password=StrongPassword123!;"
  dotnet user-secrets set "JwtSettings:Secret" "SuperSecretKeyForDevelopmentOnlyDoNotUseInProd123456"
  ```

- **Môi trường CI/CD, Staging và Production**:
  Truyền thông qua biến môi trường (Environment Variables) hoặc Azure Key Vault / AWS Secrets Manager / HashiCorp Vault. Trong code ASP.NET Core, hệ thống cấu hình sẽ tự động ưu tiên nạp biến môi trường đè lên `appsettings.json`.

---

### 9.6. Mẫu CI/CD Pipeline Tự Động Hóa Cho ASP.NET Core

Tệp cấu hình mẫu GitHub Actions (`.github/workflows/dotnet-ci.yml`) tự động kích hoạt khi có Pull Request vào nhánh `develop` hoặc `master`:

```yaml
name: ASP.NET Core CI Pipeline

on:
  pull_request:
    branches: [ develop, master ]
  push:
    branches: [ develop ]

jobs:
  build-and-test:
    runs-on: ubuntu-latest

    steps:
    - name: Checkout code
      uses: actions/checkout@v4

    - name: Setup .NET SDK
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '8.0.x'

    - name: Restore dependencies
      run: dotnet restore

    - name: Verify Code Formatting
      run: dotnet format --verify-no-changes --verbosity diagnostic

    - name: Build Project
      run: dotnet build --no-restore --configuration Release

    - name: Run Unit Tests
      run: dotnet test --no-build --configuration Release --verbosity normal --collect:"XPlat Code Coverage"

    - name: Generate EF Core SQL Migration Script (Dry-run check)
      run: |
        dotnet tool install --global dotnet-ef
        dotnet ef migrations script --idempotent --output ./migration-check.sql
```
