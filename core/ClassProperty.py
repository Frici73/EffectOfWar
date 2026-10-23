lazy from functools import update_wrapper
lazy from typing import Any, Callable

class classproperty:
    def __init__(self, func: Callable[..., Any]) -> None:
        self.func = func
        update_wrapper(self, func) # type: ignore

    def __get__(self, instance: object, owner: type | None = None) -> Any:
        if owner is None:
            raise AttributeError("Cannot resolve the owning class.")

        return self.func(owner)
