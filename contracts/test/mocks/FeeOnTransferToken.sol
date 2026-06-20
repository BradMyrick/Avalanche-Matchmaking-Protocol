// SPDX-License-Identifier: Apache-2.0
pragma solidity ^0.8.33;

import "openzeppelin-contracts/contracts/token/ERC20/ERC20.sol";

/// @dev Minimal fee-on-transfer ERC20 for the Phase 1.5 test. Each transfer
///      delivers only 99% of the sent amount to the recipient (1% is silently
///      dropped), so `balanceOf(receiver)` grows by less than the amount sent.
///      This lets the test prove AMPRegistry tracks actual received stakes
///      rather than the nominal transfer amount.
contract FeeOnTransferToken is ERC20 {
    constructor() ERC20("FeeToken", "FEE") {
        _mint(msg.sender, 1_000_000 ether);
    }

    function transfer(address to, uint256 amount) public override returns (bool) {
        uint256 delivered = (amount * 99) / 100;
        _transfer(_msgSender(), to, delivered);
        return true;
    }

    function transferFrom(address from, address to, uint256 amount) public override returns (bool) {
        uint256 delivered = (amount * 99) / 100;
        _spendAllowance(from, _msgSender(), amount);
        _transfer(from, to, delivered);
        return true;
    }
}
