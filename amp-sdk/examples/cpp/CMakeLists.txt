cmake_minimum_required(VERSION 3.10)
project(AmpCppSDK)

set(CMAKE_CXX_STANDARD 14)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# --- Cap'n Proto detection: CMake package first, then pkg-config fallback ---

find_package(CapnProto REQUIRED)

# Go from sdk/examples/cpp -> sdk
get_filename_component(SDK_ROOT "${CMAKE_CURRENT_SOURCE_DIR}/../.." ABSOLUTE)

# Cap'n Proto schema config
set(CAPNPC_SRC_PREFIX "${SDK_ROOT}/schemas")
set(CAPNPC_OUTPUT_DIR "${SDK_ROOT}/schemas/generated/cpp")
file(MAKE_DIRECTORY "${CAPNPC_OUTPUT_DIR}")
include_directories(${CAPNPC_OUTPUT_DIR})

# Schemas live in sdk-root/schemas
set(SCHEMA_FILES
    ${SDK_ROOT}/schemas/game_core.capnp
    ${SDK_ROOT}/schemas/amp_telemetry.capnp
    ${SDK_ROOT}/schemas/game_types.capnp
    ${SDK_ROOT}/schemas/match.capnp
    ${SDK_ROOT}/schemas/player_profile.capnp
    ${SDK_ROOT}/schemas/matchmaking_rules.capnp
    ${SDK_ROOT}/schemas/game_registry.capnp
    ${SDK_ROOT}/schemas/inventory.capnp
    ${SDK_ROOT}/schemas/tournament.capnp
    ${SDK_ROOT}/schemas/security.capnp
    ${SDK_ROOT}/schemas/service.capnp
)

capnp_generate_cpp(SCHEMA_SRCS SCHEMA_HDRS ${SCHEMA_FILES})

add_library(amp_sdk STATIC
    ${SCHEMA_SRCS}
    ${SDK_ROOT}/cpp/src/client.cpp
)

target_link_libraries(amp_sdk PUBLIC CapnProto::capnp-rpc CapnProto::capnp CapnProto::kj-async CapnProto::kj)
target_include_directories(amp_sdk PUBLIC ${SDK_ROOT}/cpp/include)

add_executable(amp_test src/main.cpp)
target_link_libraries(amp_test PRIVATE amp_sdk)
