import cv2 as cv
import mediapipe as mp
import numpy as np
import joblib
import socket

from mediapipe.tasks import python
from mediapipe.tasks.python import vision


import time


# ============================================================
# MEDICIÓN DE RENDIMIENTO
# ============================================================

tiempos = {
    "captura": 0,
    "conversion": 0,
    "mediapipe": 0,
    "clasificacion": 0,
    "udp": 0,
    "visualizacion": 0,
    "total": 0
}

frames = 0
inicio_medicion = time.perf_counter()

# ============================================================
# MODELO
# ============================================================

model = joblib.load("Python/Models/digit_model.joblib")
#Para inferencia, al ser un solo frame es más optimo no paralelizar con -1
model.set_params(n_jobs=1)
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

cap.set(cv.CAP_PROP_FRAME_WIDTH, 640)
cap.set(cv.CAP_PROP_FRAME_HEIGHT, 480)
cap.set(cv.CAP_PROP_FPS, 30)

cap.set(cv.CAP_PROP_BUFFERSIZE, 1)

t0 = time.perf_counter()

ret, frame = cap.read()

tiempos["captura"] = (time.perf_counter() - t0) * 1000




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
    # Crear 84 características
    # izquierda = 42
    # derecha = 42
    # --------------------------------------------------------

    # ============================================================
    # EXTRAER Y CLASIFICAR UNA MANO
    # ============================================================

    if len(result.hand_landmarks) > 0:

        hand = result.hand_landmarks[0]

        handedness = result.handedness[0][0].category_name

        # Referencia: muñeca
        x_base = hand[0].x
        y_base = hand[0].y

        features = []
        landmarksTracking = []

        for idx, landmark in enumerate(hand):

            # Coordenadas relativas a la muñeca
            x = landmark.x - x_base
            y = landmark.y - y_base

            # Mismo flip que en trainDataset.py
            if handedness == "Left":
                x = -x

            features.extend([x, y])

            # Datos que se envían a Unity
            if idx == 0 or idx == 9:

                landmarksTracking.extend([
                    landmark.x,
                    landmark.y,
                    landmark.z
                ])

        # Verificar las 42 características
        X = np.array(
            [features],
            dtype=np.float32
        )


        probabilities = model.predict_proba(X)[0]

        prediction = model.classes_[np.argmax(probabilities)]

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

        # Enviar a Unity
        message = str(prediction) + "|" + str(landmarksTracking)

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