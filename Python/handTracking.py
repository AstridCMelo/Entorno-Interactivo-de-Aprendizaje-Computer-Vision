import cv2 as cv
import mediapipe as mp
import numpy as np
import joblib
import socket

from mediapipe.tasks import python
from mediapipe.tasks.python import vision


# ============================================================
# MODELO
# ============================================================

model = joblib.load("Python/Models/digit_model.joblib")

print("Modelo cargado correctamente")


# ============================================================
# MEDIAPIPE
# ============================================================

base_options = python.BaseOptions(
    model_asset_path="Python/hand_landmarker.task"
)

options = vision.HandLandmarkerOptions(
    base_options=base_options,
    num_hands=2,
    min_hand_detection_confidence=0.7,
    min_hand_presence_confidence=0.7,
    min_tracking_confidence=0.7
)

detector = vision.HandLandmarker.create_from_options(options)


# ============================================================
# UDP
# ============================================================

sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

serverAddressPort = ("127.0.0.1", 5052)


# ============================================================
# WEBCAM
# ============================================================

cap = cv.VideoCapture(0)

if not cap.isOpened():
    print("No se pudo abrir la webcam")
    exit()

cap.set(cv.CAP_PROP_FRAME_WIDTH, 800)
cap.set(cv.CAP_PROP_FRAME_HEIGHT, 800)


# ============================================================
# LOOP
# ============================================================

while cap.isOpened():

    ret, frame = cap.read()

    if not ret:
        print("No se pudo capturar el frame")
        break

    # BGR → RGB
    frame_rgb = cv.cvtColor(frame, cv.COLOR_BGR2RGB)

    mp_image = mp.Image(
        image_format=mp.ImageFormat.SRGB,
        data=frame_rgb
    )

    # Detectar manos
    result = detector.detect(mp_image)

    # --------------------------------------------------------
    # Crear 126 características
    # izquierda = 63
    # derecha = 63
    # --------------------------------------------------------

    left_landmarks = [0.0] * 63
    right_landmarks = [0.0] * 63

    for idx in range(len(result.hand_landmarks)):

        hand = result.hand_landmarks[idx]

        handedness = result.handedness[idx][0].category_name

        landmarks = []

        for landmark in hand:
            landmarks.extend([
                landmark.x,
                landmark.y,
                landmark.z
            ])

        if handedness == "Left":
            left_landmarks = landmarks

        elif handedness == "Right":
            right_landmarks = landmarks


    # --------------------------------------------------------
    # Solo predecir si hay al menos una mano
    # --------------------------------------------------------

    if len(result.hand_landmarks) > 0:

        features = left_landmarks + right_landmarks

        X = np.array(
            features,
            dtype=np.float32
        ).reshape(1, -1)

        # Predicción
        prediction = model.predict(X)[0]

        # Confianza
        probabilities = model.predict_proba(X)[0]
        confidence = np.max(probabilities)

        # Mostrar en pantalla
        cv.putText(
            frame,
            f"Digito: {prediction}",
            (30, 50),
            cv.FONT_HERSHEY_SIMPLEX,
            1.2,
            (0, 255, 0),
            3
        )

        cv.putText(
            frame,
            f"Confianza: {confidence * 100:.1f}%",
            (30, 90),
            cv.FONT_HERSHEY_SIMPLEX,
            0.8,
            (0, 255, 0),
            2
        )

        # ----------------------------------------------------
        # ENVIAR A UNITY
        # ----------------------------------------------------

        message = str(prediction)

        sock.sendto(
            message.encode("utf-8"),
            serverAddressPort
        )


    else:

        cv.putText(
            frame,
            "No se detecta mano",
            (30, 50),
            cv.FONT_HERSHEY_SIMPLEX,
            1,
            (0, 0, 255),
            2
        )


    cv.imshow("Digit Recognition", frame)


    # Q para salir
    if cv.waitKey(1) & 0xFF == ord("q"):
        break


# ============================================================
# CERRAR
# ============================================================

cap.release()
cv.destroyAllWindows()
sock.close()