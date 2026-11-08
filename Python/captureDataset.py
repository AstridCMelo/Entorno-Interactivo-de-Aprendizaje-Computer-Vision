import cv2 as cv
import mediapipe as mp
import os

from mediapipe.tasks import python
from mediapipe.tasks.python import vision

# ============================================================
# CONFIGURACIÓN
# ============================================================

ruta_principal = os.path.join(
    os.path.dirname(os.path.abspath(__file__)),
    "Gestures"
)

gesto = input("¿Qué gesto deseas capturar? (0, 1, 2...): ")

ruta_gesto = os.path.join(ruta_principal, gesto)
os.makedirs(ruta_gesto, exist_ok=True)

cont = 1

while os.path.exists(os.path.join(ruta_gesto, f"screenshot{cont}.jpg")):
    cont += 1


# ============================================================
# MEDIAPIPE
# ============================================================

base_options = python.BaseOptions(
    model_asset_path=os.path.join(
        os.path.dirname(os.path.abspath(__file__)),
        "hand_landmarker.task"
    )
)

options = vision.HandLandmarkerOptions(
    base_options=base_options,
    running_mode=vision.RunningMode.VIDEO,
    num_hands=1
)

detector = vision.HandLandmarker.create_from_options(options)


# ============================================================
# WEBCAM
# ============================================================

cap = cv.VideoCapture(0)

cap.set(cv.CAP_PROP_FRAME_WIDTH, 1920)
cap.set(cv.CAP_PROP_FRAME_HEIGHT, 1080)

if not cap.isOpened():
    print("No se pudo abrir la webcam")
    exit()

timestamp = 0



# ============================================================
# LOOP
# ============================================================

while cap.isOpened():

    ret, frame = cap.read()

    if not ret:
        break

    original_frame = frame.copy()

    # Convertir imagen para MediaPipe
    rgb = cv.cvtColor(frame, cv.COLOR_BGR2RGB)

    mp_image = mp.Image(
        image_format=mp.ImageFormat.SRGB,
        data=rgb
    )

    # Detectar landmarks
    timestamp += 1

    results = detector.detect_for_video(
        mp_image,
        timestamp
    )

    # Comprobar detección
    mano_detectada = len(results.hand_landmarks) > 0

    # Dibujar landmarks
    if mano_detectada:

        for hand_landmarks in results.hand_landmarks:

            for landmark in hand_landmarks:

                x = int(landmark.x * frame.shape[1])
                y = int(landmark.y * frame.shape[0])

                cv.circle(frame, (x, y), 5, (0, 255, 0), -1)

            connections = mp.tasks.vision.HandLandmarksConnections.HAND_CONNECTIONS

            for connection in connections:

                start = hand_landmarks[connection.start]
                end = hand_landmarks[connection.end]

                x1 = int(start.x * frame.shape[1])
                y1 = int(start.y * frame.shape[0])

                x2 = int(end.x * frame.shape[1])
                y2 = int(end.y * frame.shape[0])

                cv.line(frame, (x1, y1), (x2, y2), (255, 0, 0), 2)

        cv.putText(
            frame,
            "HAND DETECTED - SPACE TO SAVE",
            (20, 40),
            cv.FONT_HERSHEY_SIMPLEX,
            0.8,
            (0, 255, 0),
            2
        )

    else:

        cv.putText(
            frame,
            "NO HAND DETECTED",
            (20, 40),
            cv.FONT_HERSHEY_SIMPLEX,
            0.8,
            (0, 0, 255),
            2
        )

    cv.imshow("Gesture Capture", frame)

    key = cv.waitKey(1) & 0xFF

    # ESPACIO: guardar solamente si detecta una mano
    if key == 32:

        if mano_detectada:

            ruta_imagen = os.path.join(
                ruta_gesto,
                f"screenshot{cont}.jpg"
            )

            # Guardar frame ORIGINAL, sin landmarks
            cv.imwrite(ruta_imagen, original_frame)

            print(f"Imagen guardada: {ruta_imagen}")

            cont += 1

        else:
            print("No se guardó: no se detectó ninguna mano.")

    elif key == 27:
        break


# ============================================================
# CERRAR
# ============================================================

cap.release()
detector.close()
cv.destroyAllWindows()