import cv2 as cv
import mediapipe as mp
import numpy as np
import joblib
import socket

from mediapipe.tasks import python
from mediapipe.tasks.python import vision
from threadpoolctl import threadpool_limits


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

# Limitar RandomForest a un solo hilo
model.n_jobs = 1
threadpool_limits(limits=1)

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

depth_suavizado = 0.0

# Antes del while
ultimo_tiempo_clasificacion = 0
intervalo_clasificacion = 0.1  # 100 ms
ultima_prediccion = -1

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


        # ========================================================
        # ESTIMACIÓN DE PROFUNDIDAD MEDIANTE LA PALMA
        # ========================================================

        h, w, _ = frame.shape

        # Landmarks 5 y 17: extremos de la palma
        punto5 = hand[5]
        punto17 = hand[17]

        # Convertir coordenadas normalizadas a píxeles
        x5 = punto5.x * w
        y5 = punto5.y * h

        x17 = punto17.x * w
        y17 = punto17.y * h

        # Distancia euclidiana entre ambos puntos
        palm_width = np.sqrt(
            (x17 - x5)**2 +
            (y17 - y5)**2
        )

        # ========================================================
        # CALIBRACIÓN
        # ========================================================

        # Valores iniciales: debes medirlos con tu cámara
        size_min = 60    # Palma alejada
        size_max = 180   # Palma cercana

        depth_raw = np.clip(
            (palm_width - size_min) / (size_max - size_min),
            0.0,
            1.0
        )

        # ========================================================
        # SUAVIZADO
        # ========================================================

        alpha = 0.25

        depth_suavizado = (
            alpha * depth_raw +
            (1 - alpha) * depth_suavizado
        )

        depth = depth_suavizado

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

        tiempo_actual = time.perf_counter()


        if tiempo_actual - ultimo_tiempo_clasificacion >= intervalo_clasificacion:

            probabilities = model.predict_proba(X)[0]

            ultima_prediccion = model.classes_[np.argmax(probabilities)]

            ultimo_tiempo_clasificacion = tiempo_actual

        prediction = ultima_prediccion


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
        message = (
            str(prediction) + "|" +
            str(landmarksTracking) + "|" +
            str(float(depth))
        )

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