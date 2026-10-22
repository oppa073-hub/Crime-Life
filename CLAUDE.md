# CLAUDE.md

## 프로젝트 개요
- 게임: Crime Life — 범죄자 심즈 (생활 시뮬 + 범죄 액션)
- 엔진: Unity 6 (6000.3.5f2)
- 시점/플랫폼: 3D, 심즈식 탑다운 카메라 + 클릭 이동(NavMesh) / 플랫폼은 미정(모바일 또는 PC — 확정 후 수정)
- 핵심 루프: 욕구 관리 → 타겟 탐색 → 범죄 실행 → 도주 → 장물 처분/정산 → 반복
- 게임 비전: 심즈 + GTA. 플레이어 집을 꾸미고 거기서 생활한다. 도시는 별도 씬이고, 다른 캐릭터와 상호작용/관계를 쌓는다. 범죄로 먹고사는 게 생계 수단이다. (범죄 시스템은 이 비전의 일부일 뿐이고, 집 꾸미기·관계·생활비가 핵심 축)
- 현재 단계: MVP 프로토타입 완료(욕구 3개 + 범죄 2종 + NPC 5명 + 경찰 + 정산). 지금은 "비전 구현" 단계: A 구조(씬 분리/저장) → B 집 꾸미기 → C 도시 블록아웃 → D 관계 → E 생활비 → F 에셋 교체. 에셋은 박스(블록아웃)로 먼저 만들고 나중에 프리팹만 갈아 끼운다.

## 작업 원칙
- 요청한 범위만 수정한다. 관련 없는 파일이나 코드는 리팩터링하지 않는다.
- 큰 변경 전에는 계획을 먼저 짧게 설명하고 진행한다.
- 새 시스템을 만들기 전에 기존 코드에 비슷한 것이 있는지 먼저 확인한다.
- 모르는 Unity API는 추측하지 말고 확인하거나 물어본다.

## 절대 건드리지 말 것
- `.meta` 파일은 직접 수정/삭제/생성하지 않는다. (GUID 꼬임 방지)
- `Library/`, `Temp/`, `Logs/`, `obj/`, `UserSettings/` 는 읽지도 수정하지도 않는다.
- `ProjectSettings/`는 요청 없이 수정하지 않는다.
- 씬(`.unity`)과 프리팹(`.prefab`) 파일은 텍스트로 직접 편집하지 않는다. 필요하면 에디터 스크립트나 수정 방법을 안내한다.

## 폴더 구조
```
Assets/
  _Project/
    Scripts/       # CrimeLife.Runtime.asmdef (EditMode 테스트가 참조)
      Core/        # GameClock, SimsCameraRig, GameSession(씬 전환/저장), SaveData, SpawnPoint
      Needs/       # 욕구 시스템 (포만감/에너지/평온)
      Build/       # 집 꾸미기: LotGrid(순수 격자 로직), HomeLot, BuildModeController, FurnitureDefinition/Catalog
      Economy/     # 생활비: BillingLogic(순수), LivingCostConfig, BillingManager(청구/납부/연체)
      Social/      # 관계: RelationshipLogic(순수), Config, RelationshipBook(저장), PlayerSocialController(대화/선물)
      Interaction/ # 가구 상호작용 (Interactable, PlayerInteractor)
      Player/      # 클릭 이동
      Crime/       # ICrimeAction, 소매치기, 지갑, 수배 열기
      NPC/         # NpcProfile, 일과표, Waypoint
      UI/          # UI Toolkit HUD 스크립트
    UI/            # UXML, USS, PanelSettings
    Tests/EditMode/
    Data/          # ScriptableObject 데이터 (NeedsConfig, CrimeConfig, Npc_*, Interactions/)
    Prefabs/ Art/ Audio/           # Art는 머티리얼만 있음
Assets/Scenes/   # City(도시), Home(플레이어 집). Build Settings에 둘 다 등록돼 있다
```
- 입력은 Input System을 쓴다. 클릭은 `Update` 폴링 대신 `InputAction` 콜백으로 받는다.
- 욕구는 모두 "충족도"(100 = 좋음)다. 스트레스는 평온(Calm)이 줄어드는 것으로 표현한다.
- 씬 구성(오브젝트 생성, 컴포넌트 연결, NavMesh 베이크)은 Unity MCP로 한다. 가구 등 장애물을 추가하면 `Ground`의 NavMeshSurface를 다시 베이크한다.
- `GameClock`과 `NeedsConfig`에 시간 배율이 따로 있다. 바꿀 때 둘을 같이 맞춘다.

- 외부 에셋(에셋스토어 구매품, 다운로드한 에셋 팩)은 `Assets/_External/`에 넣는다. 이 폴더는 .gitignore로 제외되어 공개 저장소에 올라가지 않는다. 내가 직접 만든 에셋만 `_Project/`에 둔다.

## 코드 컨벤션
- 언어: C#, 클래스/메서드는 PascalCase, private 필드는 `_camelCase`
- 인스펙터 노출은 `public` 대신 `[SerializeField] private` 사용
- `Update()` 안에서 `GetComponent`, `Find`, LINQ, 문자열 연결 금지 (모바일 성능)
- 시스템 간 통신은 직접 참조보다 C# event 또는 ScriptableObject 이벤트 채널 사용
- 수치/밸런스 값은 코드에 하드코딩하지 말고 ScriptableObject로 분리
- 싱글톤은 GameManager 등 꼭 필요한 곳에만 사용
- 주석은 "왜 이렇게 했는지"만 작성하고, 한국어 가능

## 설계 규칙
- **생활 ↔ 범죄 연결**: 욕구 상태는 범죄 성공률에 영향을 주고, 범죄 결과(수배, 부상, 스트레스)는 생활 시스템에 되돌아온다. 한쪽만 구현하는 코드는 반드시 연결 지점(훅)을 남긴다.
- NPC는 데이터 기반으로 만든다. (성격, 일과표, 소지금을 ScriptableObject로 정의)
- 범죄 종류는 공통 인터페이스(예: `ICrimeAction`)로 만들어 새 범죄를 쉽게 추가한다.
- 먼저 동작하는 최소 버전을 만들고 확장한다. 과설계 금지.

## 빌드/테스트
- 에디터에서 플레이 테스트가 기본이다. 가능하면 순수 로직(욕구 계산, 수배 판정)은 `MonoBehaviour`와 분리해 EditMode 테스트가 가능하게 짠다.
- 컴파일 에러가 없는 상태로 작업을 마친다. 확인할 수 없으면 확인하지 못했다고 명시한다.

## 응답 스타일
- 한국어로 답한다.
- 변경한 파일 목록과 Unity 에디터에서 해야 할 수동 작업(컴포넌트 붙이기, 인스펙터 연결 등)을 마지막에 정리한다.

## 씬 전환과 저장
- 상태(지갑/욕구/열기/시계)는 각 컴포넌트가 들고 있고, `ISaveable`을 구현해 `SaveData`에 넣고 꺼낸다. `GameSession`(싱글톤, 자동 생성)이 씬 전환 때 옮겨 담는다. 새 상태를 저장하려면 `ISaveable`을 구현하고 `SaveData`에 필드를 추가한다.
- 문은 `InteractionConfig`의 `LoadScene`/`SpawnId`로 정의한다. 도착 씬에는 `SpawnPoint` + `SceneSpawner`가 있어야 한다.
- F5 저장 / F9 불러오기. 저장 파일은 `Application.persistentDataPath/save.json`.
- 씬을 새로 만들 때: Build Settings에 등록하고, NavMeshSurface는 씬마다 별도 NavMeshData 에셋(`Data/NavMesh_<씬>.asset`)으로 굽는다 (공유하면 서로 덮어쓴다).
- NPC 상태는 아직 저장하지 않는다 (도시에 들어올 때마다 일과표 위치로 초기화).

## 현재 TODO
- [x] 욕구 시스템 기본 구현
- [x] 플레이어 이동/상호작용
- [x] 범죄 1종 (소매치기) 프로토타입
- [x] NPC 1명 일과표 AI
- [x] 범죄 2종째 추가 (강도: Shift+클릭, 소매치기: 클릭)
- [x] NPC 5명으로 확장 (민수/지연/철수/영희/동수, 장소 10곳)
- [x] 수배 열기에 경찰 반응 연결 (PoliceOfficer: 열기 30 이상 추격, 10 미만 포기, 체포 시 벌금/평온 감소/경찰서 이동)
- [x] 범죄 쿨다운(당한 NPC는 30초 경계), 목격/신고(반경 8m, 경계심 비례, 신고 시 열기 +15)
- [x] 장물 처분/정산 (범죄 수익은 장물, 처분소에서 70%로 현금화, 체포 시 장물 전부 압수, 음식은 현금 20)
- [x] 마우스 클릭 입력과 HUD 표시 확인 (가상 마우스 입력 + 스크린샷으로 검증)

### 비전 구현 로드맵
- [x] A. 구조: 집/도시 씬 분리, 문으로 이동, 상태 유지, F5/F9 저장
- [x] B-1. 집 꾸미기(가구): 14x14 격자, B로 건축 모드, 카탈로그 구입, R 회전, 클릭 이동(무료), 우클릭 판매(50%), 배치 저장. 가구는 SO(`FurnitureDefinition`)+프리팹이고, 길찾기는 NavMeshObstacle(carve)로 처리한다
- [ ] B-2. 벽/문/바닥 편집 (심즈의 핵심이지만 방 판정/건축 규칙이 따라와서 따로 진행)
- [x] C. 도시 블록아웃: 십자 도로 + 건물 박스 10개(NPC 집 5, 회사, 카페, 시장, 경찰서, 내 집) + 공원/광장 바닥, 건물 이름표(`PlaceLabels`). 건물 몸체는 NPC 장소(Waypoint) 뒤쪽에 두어서 waypoint가 곧 건물 출입구다. 건물은 NavMeshModifier로 "걸을 수 없음" 처리(지붕 위로 못 올라감)
- [x] D. 관계: NPC별 친밀도(0~100, 낯선 사람/아는 사이/친구/절친), NPC 클릭 메뉴(대화/선물/소매치기/강도), 저장. 친할수록 경계심↓(털기 쉽고 목격 신고도 덜 함), 친한 사람을 털면 친밀도 폭락 + 죄책감으로 평온↓. NPC 쫓아가기는 `NpcChaser`를 범죄/사교가 공유. UI 위 클릭이 월드로 새지 않도록 `UiHit`로 막는다
- [ ] D-2(선택). 사교 욕구(4번째 욕구)로 대화의 동기 만들기, NPC끼리의 관계, 관계가 정보/장물 거래로 이어지게
- [x] E. 생활비: 게임 시간 48시간마다 월세 80 + 공과금 20 청구, 집 우편함에서 납부. 기한(24시간) 넘기면 연체료 20% + 평온 -30 + 가장 비싼 가구 압류(도시에서 밀린 건 집에 들어올 때 적용). `GameClock`에 Day/TotalHours 추가. 음식은 냉장고 20
- [ ] F. 에셋 교체: 박스 → 모델/애니메이션 (`Assets/_External/`에 두고 gitignore)
  - [x] F-1 캐릭터: `CharacterVisual` 슬롯(모델 프리팹 또는 색 캡슐 대체) + `CharacterLocomotion.controller`(속도 0/3.5/6 블렌드: 대기/걷기/달리기) + `CharacterAnimator`. 플레이어 `Character_10_1_1`, 민수 7_1_1, 지연 11_1_2, 철수 B_1, 영희 4_1_1, 동수 9_1_1, 경찰 `PoliceMan_B_1_1`
  - [ ] F-2 도시: 건물/도로/소품 프리팹으로 교체 (NavMesh와 NPC 출입구 위치 재조정 필요)
  - [ ] F-3 집 가구: 실내 모델이 있는지 확인 후 결정
  - 외부 에셋 규칙: 씬/프리팹이 `_External`을 직접 참조하면 에셋 없는 환경에서 깨진다. `CharacterVisual`처럼 "참조가 비면 대체물"이 되는 슬롯 구조로 만들어 둔다. 에셋 임포트 시 `.blend`는 Blender가 없으면 에러가 나므로 프로젝트에 넣지 않는다(FBX만 쓴다)
  - 에셋 팩의 `.unitypackage`를 임포트하면 URP 설정(`Assets/Settings/DefaultVolumeProfile` 등)을 덮어쓰고 `Packages/manifest.json`을 바꾼다. 임포트 후 덮어쓴 설정을 git으로 복원하고, 풀린 폴더를 `_External`로 옮긴다. 외부 팩이 요구한 패키지(ShaderGraph, VFX Graph, Adaptive Performance)는 아직 커밋하지 않았다
- [ ] 가구 배치 상태를 SaveData에 저장 (B 이후), NPC 상태 저장

## MCP 검증 팁
- 플레이 모드에서 `Application.runInBackground = true`를 코드로 켜야 에디터 포커스가 없어도 프레임이 흐른다 (프로젝트 설정은 건드리지 않는다).
- 마우스 클릭은 `InputSystem.QueueStateEvent(Mouse.current, new MouseState{...}.WithButton(...))`로 흉내 낼 수 있다. Shift는 Keyboard에 같은 방식으로 넣는다.
- 스크린샷은 `manage_camera screenshot`에 `output_folder: "Captures"`로 찍는다 (Assets 밖이라 .meta가 안 생긴다).
- 에디터가 포커스를 잃은 상태에서는 가상 마우스 위치가 다음 프레임에 (0,0)으로 읽힌다. 호버가 필요한 테스트는 위치 이벤트를 보낸 뒤 1초 안에 같은 위치로 클릭 이벤트(위치+버튼 누름+뗌)를 한 번에 보낸다.
- `execute_code`는 `using`을 못 쓰고 구형 컴파일러라 중첩 튜플이 안 된다. 전체 이름 + 문자열 파싱으로 쓴다. 에셋 삭제(`AssetDatabase.DeleteAsset`)는 안전장치로 막혀 있다.
