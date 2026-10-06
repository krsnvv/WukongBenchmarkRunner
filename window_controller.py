import ctypes
import time
from ctypes import wintypes
from turtle import title


user32 = ctypes.WinDLL(
    "user32",
    use_last_error=True,
)
user32.keybd_event.argtypes = [
    wintypes.BYTE,
    wintypes.BYTE,
    wintypes.DWORD,
    ctypes.c_size_t,
]

user32.keybd_event.restype = None

# -----------------------------
# Константы
# -----------------------------

SW_RESTORE = 9

INPUT_KEYBOARD = 1

KEYEVENTF_KEYUP = 0x0002

VK_RETURN = 0x0D
VK_TAB = 0x09
VK_UP = 0x26
VK_DOWN = 0x28
VK_LEFT = 0x25
VK_RIGHT = 0x27
VK_SPACE = 0x20


# -----------------------------
# Структуры SendInput
# -----------------------------

class KEYBDINPUT(ctypes.Structure):
    _fields_ = [
        ("wVk", wintypes.WORD),
        ("wScan", wintypes.WORD),
        ("dwFlags", wintypes.DWORD),
        ("time", wintypes.DWORD),
        ("dwExtraInfo", ctypes.c_void_p),
    ]


class INPUT_UNION(ctypes.Union):
    _fields_ = [
        ("ki", KEYBDINPUT),
    ]


class INPUT(ctypes.Structure):
    _anonymous_ = ("union",)

    _fields_ = [
        ("type", wintypes.DWORD),
        ("union", INPUT_UNION),
    ]

class RECT(ctypes.Structure):
    _fields_ = [
        ("left", wintypes.LONG),
        ("top", wintypes.LONG),
        ("right", wintypes.LONG),
        ("bottom", wintypes.LONG),
    ]


# -----------------------------
# Поиск окна Unreal
# -----------------------------

EnumWindowsProc = ctypes.WINFUNCTYPE(
    wintypes.BOOL,
    wintypes.HWND,
    wintypes.LPARAM,
)


def get_window_title(hwnd):
    length = user32.GetWindowTextLengthW(hwnd)

    buffer = ctypes.create_unicode_buffer(
        length + 1
    )

    user32.GetWindowTextW(
        hwnd,
        buffer,
        len(buffer),
    )

    return buffer.value


def get_window_class(hwnd):
    buffer = ctypes.create_unicode_buffer(256)

    user32.GetClassNameW(
        hwnd,
        buffer,
        len(buffer),
    )

    return buffer.value

def find_benchmark_window():
    result = None

    @EnumWindowsProc
    def callback(hwnd, lparam):
        nonlocal result

        if not user32.IsWindowVisible(hwnd):
            return True

        title = get_window_title(hwnd)
        class_name = get_window_class(hwnd)

        if class_name == "UnrealWindow":
            print(
                "Найден UnrealWindow:",
                repr(title),
                "HWND:",
                hwnd,
            )

            if title.strip().lower() == "b1":
                result = hwnd
                return False

        return True

    user32.EnumWindows(
        callback,
        0,
    )

    return result
def focus_window(hwnd):
    user32.ShowWindow(
        hwnd,
        SW_RESTORE,
    )

    time.sleep(0.3)

    success = user32.SetForegroundWindow(
        hwnd
    )

    return bool(success)


def press_key(vk_code):
    KEYEVENTF_KEYUP = 0x0002

    # Нажатие клавиши
    user32.keybd_event(
        vk_code,
        0,
        0,
        0,
    )

    time.sleep(0.05)

    # Отпускание клавиши
    user32.keybd_event(
        vk_code,
        0,
        KEYEVENTF_KEYUP,
        0,
    )

def click_relative(hwnd, x_ratio: float, y_ratio: float):
    rect = RECT()

    if not user32.GetWindowRect(hwnd, ctypes.byref(rect)):
        raise ctypes.WinError(ctypes.get_last_error())

    width = rect.right - rect.left
    height = rect.bottom - rect.top

    x = rect.left + int(width * x_ratio)
    y = rect.top + int(height * y_ratio)

    print(f"Клик: x={x}, y={y}")

    user32.SetCursorPos(x, y)

    time.sleep(0.3)

    MOUSEEVENTF_LEFTDOWN = 0x0002
    MOUSEEVENTF_LEFTUP = 0x0004

    user32.mouse_event(
        MOUSEEVENTF_LEFTDOWN,
        0,
        0,
        0,
        0,
    )

    time.sleep(0.05)

    user32.mouse_event(
        MOUSEEVENTF_LEFTUP,
        0,
        0,
        0,
        0,
    )
def press_enter():
    press_key(VK_RETURN)