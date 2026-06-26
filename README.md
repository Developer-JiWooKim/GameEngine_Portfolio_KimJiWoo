# 게임 엔진 포트폴리오 과제

**유니티 버전:** 6.4.9.f1

---

## 프로젝트 개요

플레이어는 미로에 갇혀 있다. 미로 곳곳에 흩어진 **조각(열쇠)** 을 모두 모으면, **Goal Point** 가 나타난다. 그  **Goal Point**에 도달하면 미로에서 탈출할 수 있다.

미로는 특정 커맨드를 입력하여 전환할 수 있다.
- **Physical**
- **Arcane**

두 레이어는 완전히 다른 벽 구조를 갖고 있어서, 한쪽에서 막힌 길이 다른 쪽에서는 열려 있을 수 있다. 플레이어는 언제든 두 레이어를 오갈 수 있고, 미로를 떠도는 몬스터를 피해 길을 찾아 조각을 모아야 한다.

| 항목 | 내용 |
|---|---|
| 엔진 | Unity 6 |
| 렌더 파이프라인 | URP |
| 장르 | 탑다운 미로 탐험 / 추격 회피 |
| 미로 크기 | 20 x 20 (고정) |

---

## 시연 방법

1. Unity 6(URP)로 프로젝트 열기(Version:6.4.9f1)
2. `Assets/MyAssets/Scenes/` 의 `GameEngineAssignment` 씬 열고 Play
3. 타이틀 화면에서 **Start** 버튼 클릭 → 20x20 고정 크기 미로로 바로 게임 시작

---

## 게임 목표 / 클리어 조건

1. 미로 곳곳에 흩어진 **열쇠(빛나는 구체) 5개**를 전부 수집
2. 5개를 모두 모으면 미로의 오른쪽 위 구석에 **Goal Point(황금색 구체)** 생성
3. **Goal Point(황금색 구체)** 에 도달하면 **클리어**
4. 몬스터에게 발각되면 플레이어를 추적 → 공격범위 안에 들어와 공격당해 **HP(3)가 0이 되면 게임 오버**
5. 클리어/게임 오버 후 결과 화면에서 **Replay**로 재도전 가능

---

## 조작법

| 입력 | 기능 |
|---|---|
| `W` `D` `A` `S` / 방향키 | 상하좌우 이동 |
| `Tab` | 미로 레이어 전환 (Physical ↔ Arcane) |

---

## 실행 화면

### 게임 실행
<img src="Assets/MyAssets/Screenshots/Start.png" width="800">

### 게임 플레이 
<img src="Assets/MyAssets/Screenshots/Runtime.png" width="800">

### 게임 클리어
<img src="Assets/MyAssets/Screenshots/End.png" width="800">

---

## 사용한 엔진 기능

| 분류 | 적용 내용 |
|---|---|
| **Cinemachine** | `CinemachineBrain` + `CinemachineCamera`, 시작 인트로 카메라와 쿼터뷰 추적 카메라를 우선순위 기반 블렌딩으로 카메라 전환 연출, `Cinemachine Impulse`로 몬스터에게 피격 시 카메라 흔들림 |
| **조명 / 머티리얼** | Physical/Arcane 벽 머티리얼(ShaderGraph → `SG_PhysicalGrime`, `SG_ArcaneNoise`), Key/Goal Point 용 발광 머티리얼(ShaderGraph → `SG_GlowOrb`), 실시간 Point Light pulse(`PulsingLight.cs`), 레이어 전환에 맞춘 Directional Light·대기 Fog 색 전환(`LayerLightingController.cs`), 화면 일렁임 전환 효과(ShaderGraph → `SG_LayerTransitionRipple`) |
| **이동/경로 시스템** | Player: `CharacterController` 기반 이동 / Monster: `NavMeshAgent` + `NavMeshSurface`(레이어별 분리 베이크) 로 미로 Patrol, 플레이어 추적 |
| **UI** | Canvas 하위 Title/InGame/Result UI Panel을 `GameUIController.cs`가 조율(패널 전환·이벤트 연결), `DamageflashUI.cs`로 피격 시 화면 빨간색 페이드 연출 |
| **사운드** | `SoundManager` + `SoundLibrary`(ScriptableObject, AudioClip들을 보관) |
| **물리 Trigger** | 열쇠 회수, 골 포인트 도달, 몬스터 공격 범위 판정 |
| **VFX** | Particle System(사방으로 흩어지는 입자) |
| **리소스 관리** | MyAssets → Materials / Prefabs / Scripts / ShaderGraphs 등의 폴더 분리 후 관리, External 에셋(AOS Fog War)은 따로 건드리지 않음(임포트 경로 그대로 보존)|

### 추가로 사용한 시스템
- **Input System** — `PlayerInput`(Invoke C Sharp Events) 기반 이벤트 입력 처리
- **Awaitable 비동기** — 레이어 전환 시퀀스, 카메라 인트로 전환 등 시간 기반 연출 처리
- **FSM** — 몬스터 AI(Idle / Chase / Attack) 상태 관리
- **External Asset: AOS Fog War** — 플레이어 시야 기반 안개 시스템

---

## 리소스 구성

```
Assets/
	AOSFogWar/           # 외부 에셋
	MyAssets/			 # 직접 작업한 작업물
  		Materials/            	 # Materials
  		Prefabs/				 # Prefabs
		Scenes/	   				 # GameAssignment Scene
		Screenshots/ 			 # 인 게임 Screen Shots
		ScriptableObjectAssets/  # SoundLibrary Asset
  		Scripts/
			Monster/             # 몬스터 관련 .cs
			Obsolete/            # AStarPathfinder, FollowCamera ([Obsolete] 처리한 .cs)
			Player/              # 플레이어 관련 .cs
			ScriptableObject/	 # 스크립터블 오브젝트 .cs(SoundLibrary)
			UI/					 # UI 관련 .cs
    		Utility/             # 이 외의 모든 .cs
				Manager/		 # Utility안에서도 매니저는 따로 폴더로 구분    		
  		ShaderGraphs/            # Shader Graphs
		SoundClips/				 # AudioClips
```

