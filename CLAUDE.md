# CLAUDE.md

## 프로젝트 개요
- 게임: Crime Life — 범죄자 심즈 (생활 시뮬 + 범죄 액션)
- 엔진: Unity 6 (6000.3.5f2)
- 시점/플랫폼: 3D, 심즈식 탑다운 카메라 + 클릭 이동(NavMesh) / 플랫폼은 미정(모바일 또는 PC — 확정 후 수정)
- 핵심 루프: 욕구 관리 → 타겟 탐색 → 범죄 실행 → 도주 → 장물 처분/정산 → 반복
- 현재 단계: MVP 프로토타입 (욕구 3개 + 범죄 2종 + NPC 5명 범위)

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
      Core/        # GameClock, SimsCameraRig 등
      Needs/       # 욕구 시스템 (포만감/에너지/평온)
      Interaction/ # 가구 상호작용 (Interactable, PlayerInteractor)
      Player/      # 클릭 이동
      Crime/       # ICrimeAction, 소매치기, 지갑, 수배 열기
      NPC/         # NpcProfile, 일과표, Waypoint
      UI/          # UI Toolkit HUD 스크립트
    UI/            # UXML, USS, PanelSettings
    Tests/EditMode/
    Data/          # ScriptableObject 데이터 (NeedsConfig, CrimeConfig, Npc_*, Interactions/)
    Prefabs/ Scenes/ Art/ Audio/   # 아직 비어 있음
```
- 입력은 Input System을 쓴다. 클릭은 `Update` 폴링 대신 `InputAction` 콜백으로 받는다.
- 욕구는 모두 "충족도"(100 = 좋음)다. 스트레스는 평온(Calm)이 줄어드는 것으로 표현한다.
- 씬 구성(오브젝트 생성, 컴포넌트 연결, NavMesh 베이크)은 Unity MCP로 한다. 가구 등 장애물을 추가하면 `Ground`의 NavMeshSurface를 다시 베이크한다.
- `GameClock`과 `NeedsConfig`에 시간 배율이 따로 있다. 바꿀 때 둘을 같이 맞춘다.

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

## MCP 검증 팁
- 플레이 모드에서 `Application.runInBackground = true`를 코드로 켜야 에디터 포커스가 없어도 프레임이 흐른다 (프로젝트 설정은 건드리지 않는다).
- 마우스 클릭은 `InputSystem.QueueStateEvent(Mouse.current, new MouseState{...}.WithButton(...))`로 흉내 낼 수 있다. Shift는 Keyboard에 같은 방식으로 넣는다.
- 스크린샷은 `manage_camera screenshot`에 `output_folder: "Captures"`로 찍는다 (Assets 밖이라 .meta가 안 생긴다).
