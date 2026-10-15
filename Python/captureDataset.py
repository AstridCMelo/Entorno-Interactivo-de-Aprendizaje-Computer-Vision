import cv2 as cv

# ============================================================
# WEBCAM
# ============================================================

cap = cv.VideoCapture(0)

cont = 5

if not cap.isOpened():
    print("No se pudo abrir la webcam")
    exit()

cap.set(cv.CAP_PROP_FRAME_WIDTH, 1920)
cap.set(cv.CAP_PROP_FRAME_HEIGHT, 1080)


# ============================================================
# LOOP
# ============================================================

while cap.isOpened():

    ret, frame = cap.read()

    if not ret:
        print("No se pudo capturar el frame")
        break

    cv.imshow("Fraction Gesture", frame)

    key = cv.waitKey(1)

    if key == 0x20:
        cv.imwrite(f'Python/fractionGesture/fractionGesture{cont}.jpg', frame)
        cont += 1

    elif key == 27:
        break


# ============================================================
# CERRAR
# ============================================================

cap.release()
cv.destroyAllWindows()