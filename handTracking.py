
import cv2 as cv
import mediapipe as mp
from mediapipe.tasks import python
from mediapipe.tasks.python import vision

cap = cv.VideoCapture(0)
cap.set(cv.CAP_PROP_FRAME_WIDTH, 800)
cap.set(cv.CAP_PROP_FRAME_HEIGHT, 800)

mp_hands = mp.solutions.hands
mp_drawing = mp.solutions.drawing_utils

hand = mp_hands.Hands()

while cap.isOpened():
    ret, frame = cap.read()
    if not ret:
        print("No se pudo capturar el frame")
        break

    frame = cv.cvtColor(frame, cv.COLOR_BGR2RGB)
    results = hand.process(frame)
    frame = cv.cvtColor(frame, cv.COLOR_RGB2BGR)

    if results.multi_hand_landmarks:
        for hand_landmarks in results.multi_hand_landmarks:
            mp_drawing.draw_landmarks(frame, hand_landmarks, mp_hands.HAND_CONNECTIONS)
            print(hand_landmarks)

    cv.imshow('Hand Detection', frame)

    if cv.waitKey(1) & 0xFF == ord('q'):
            break

cap.release()
cv.destroyAllWindows()
