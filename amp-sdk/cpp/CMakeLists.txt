cmake_minimum_required(VERSION 3.10)
project(AmpCppSDKTests CXX)

set(CMAKE_CXX_STANDARD 20)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# The Keccak + EIP-712 tests deliberately depend on NOTHING but the C++
# standard library — no Cap'n Proto, no kj, no RPC machinery. This lets the
# test target build in minimal environments (Unreal's restricted build
# toolchain, CI images without capnp headers). If the bundled keccak256.hpp
# + eip712.cpp compile and the tests pass, the AMP outcome-signing path is
# correct for any engine integration.

# SDK root: amp-sdk/cpp -> amp-sdk -> repo root
get_filename_component(SDK_ROOT "${CMAKE_CURRENT_SOURCE_DIR}/.." ABSOLUTE)

add_executable(amp_tests
    ${CMAKE_CURRENT_SOURCE_DIR}/tests/eip712_test.cpp
    ${CMAKE_CURRENT_SOURCE_DIR}/src/eip712.cpp
)

target_include_directories(amp_tests PRIVATE
    ${CMAKE_CURRENT_SOURCE_DIR}/include
)

# Enable the test in CTest so `ctest -VV` works.
enable_testing()
add_test(NAME amp_eip712_tests COMMAND amp_tests)
