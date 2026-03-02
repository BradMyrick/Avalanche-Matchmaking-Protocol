cmake_minimum_required(VERSION 3.10)
project(AmpCppSDK)

set(CMAKE_CXX_STANDARD 14)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# --- Cap'n Proto detection: CMake package first, then pkg-config fallback ---

find_package(CapnProto REQUIRED)

# Go from sdk/cpp -> sdk -> repo root
get_filename_component(REPO_ROOT "${CMAKE_CURRENT_SOURCE_DIR}/../.." ABSOLUTE)

# Cap'n Proto schema config
set(CAPNPC_SRC_PREFIX "${REPO_ROOT}")
set(CAPNPC_OUTPUT_DIR "${CMAKE_CURRENT_BINARY_DIR}/generated")
file(MAKE_DIRECTORY "${CAPNPC_OUTPUT_DIR}")
include_directories(${CAPNPC_OUTPUT_DIR})

# Schemas live in repo-root/schemas
set(SCHEMA_FILES
    ${REPO_ROOT}/schemas/game_core.capnp
    ${REPO_ROOT}/schemas/amp_telemetry.capnp
    ${REPO_ROOT}/schemas/game_types.capnp
    ${REPO_ROOT}/schemas/match.capnp
    ${REPO_ROOT}/schemas/service.capnp
)

capnp_generate_cpp(SCHEMA_SRCS SCHEMA_HDRS ${SCHEMA_FILES})

add_library(amp_sdk STATIC
    ${SCHEMA_SRCS}
    src/AmpClient.cpp
)

target_link_libraries(amp_sdk PUBLIC CapnProto::capnp-rpc CapnProto::capnp CapnProto::kj-async CapnProto::kj)
target_include_directories(amp_sdk PUBLIC include)

add_executable(amp_test src/main.cpp)
target_link_libraries(amp_test PRIVATE amp_sdk)
