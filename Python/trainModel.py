import json
import numpy as np

from sklearn.model_selection import train_test_split
from sklearn.ensemble import RandomForestClassifier
from sklearn.metrics import accuracy_score, classification_report
import joblib


# ============================================================
# RUTA DEL JSON
# ============================================================

json_path = "Python/Models/mx_landmarks.json"


# ============================================================
# CARGAR DATASET
# ============================================================

with open(json_path, "r", encoding="utf-8") as f:
    data = json.load(f)


print("======================================")
print("DATASET CARGADO")
print("======================================")
print("Total de muestras:", len(data))


# ============================================================
# SEPARAR X E Y
# ============================================================

X = []
y = []

for fila in data:

    label = fila[0]

    # Desde fila[1] empiezan:
    # 63 landmarks de la mano izquierda
    # 63 landmarks de la mano derecha
    landmarks = fila[1:]

    # Verificar que haya exactamente 126 características
    if len(landmarks) != 126:
        print(
            f"ERROR: muestra con {len(landmarks)} características"
        )
        continue

    X.append(landmarks)
    y.append(label)


X = np.array(X, dtype=np.float32)
y = np.array(y)


print("Características por muestra:", X.shape[1])
print("Clases:", np.unique(y))


# ============================================================
# TRAIN / TEST
# ============================================================

X_train, X_test, y_train, y_test = train_test_split(
    X,
    y,
    test_size=0.20,
    random_state=42,
    stratify=y
)


print()
print("Entrenamiento:", len(X_train))
print("Prueba:", len(X_test))


# ============================================================
# RANDOM FOREST
# ============================================================

model = RandomForestClassifier(
    n_estimators=200,
    random_state=42,
    n_jobs=-1
)


print()
print("Entrenando modelo...")

model.fit(X_train, y_train)


# ============================================================
# EVALUACIÓN
# ============================================================

y_pred = model.predict(X_test)

accuracy = accuracy_score(y_test, y_pred)

print()
print("======================================")
print("RESULTADOS")
print("======================================")
print(f"Accuracy: {accuracy * 100:.2f}%")
print()
print(classification_report(y_test, y_pred))


# ============================================================
# GUARDAR MODELO
# ============================================================

model_path = "Python/Models/digit_model.joblib"

joblib.dump(model, model_path)

print("======================================")
print("MODELO GUARDADO")
print("======================================")
print(model_path)
print("======================================")

