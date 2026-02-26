cmake_minimum_required(VERSION 3.10)
project(AmpCppSDK)

set(CMAKE_CXX_STANDARD 14)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# --- Cap'n Proto detection: CMake package first, then pkg-config fallback ---

# Try the official CapnProto CMake package
find_package(CapnProto QUIET)

if (CapnProto_FOUND)
    message(STATUS "Using CapnProto CMake package")
    include_directories(${CAPNP_INCLUDE_DIRS} ./include ./src)
    add_definitions(${CAPNP_DEFINITIONS})
    set(CAPNP_LIBS CapnProto::capnp-rpc CapnProto::capnp CapnProto::kj-async CapnProto::kj)
else()
    message(STATUS "CapnProto CMake package not found or unusable, falling back to pkg-config")

    find_package(PkgConfig REQUIRED)
    pkg_check_modules(CAPNP REQUIRED capnp capnp-rpc kj kj-async)

    include_directories(${CAPNP_INCLUDE_DIRS} ./include ./src)
    link_directories(${CAPNP_LIBRARY_DIRS})
    add_definitions(${CAPNP_CFLAGS_OTHER})

    set(CAPNP_LIBS ${CAPNP_LIBRARIES})
endif()

# --- Sources ---

# Generated schema files (from Cap'n Proto)
set(SCHEMA_SOURCES
    src/schemas/game_types.capnp.c++
    src/schemas/match.capnp.c++
    src/schemas/service.capnp.c++
)

# SDK implementation
set(SDK_SOURCES
    src/AmpClient.cpp
)

# --- Targets ---

# Static library for the SDK
add_library(amp_sdk STATIC ${SCHEMA_SOURCES} ${SDK_SOURCES})
target_link_libraries(amp_sdk PUBLIC ${CAPNP_LIBS})
target_include_directories(amp_sdk PUBLIC include)

# Example/test executable
add_executable(amp_test src/main.cpp)
target_link_libraries(amp_test PRIVATE amp_sdk)
