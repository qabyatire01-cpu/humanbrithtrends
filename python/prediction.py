import sys
import json

import numpy as np
from sklearn.linear_model import LinearRegression

from data_processing import load_history_dataframe, compute_gender_ratio

MODEL_NAME = "Linear Regression"

def fit_and_predict(df, start_year, end_year):
    
    X = df["year"].to_numpy().reshape(-1, 1).astype(float)
    y = df["total_births"].to_numpy().astype(float)

    model = LinearRegression()
    model.fit(X, y)

    years_to_predict = np.arange(start_year, end_year + 1).reshape(-1, 1).astype(float)
    predicted = model.predict(years_to_predict)

    predicted = np.clip(predicted, a_min=0, a_max=None)

    return {
        int(year[0]): int(round(value))
        for year, value in zip(years_to_predict, predicted)
    }

def main():
    raw_input = sys.stdin.read()

    try:
        payload = json.loads(raw_input)
    except json.JSONDecodeError as exc:
        print(json.dumps({"success": False, "error": f"Invalid input JSON: {exc}"}))
        return

    start_year = payload.get("start_year")
    end_year = payload.get("end_year")
    history_records = payload.get("history", [])

    if start_year is None or end_year is None:
        print(json.dumps({"success": False, "error": "start_year and end_year are required."}))
        return

    if start_year > end_year:
        print(json.dumps({"success": False, "error": "start_year must be less than or equal to end_year."}))
        return

    df = load_history_dataframe(history_records)

    if df.empty or len(df) < 2:
        print(json.dumps({
            "success": False,
            "error": "At least two valid years of historical data are required to train the prediction model."
        }))
        return

    try:
        totals_by_year = fit_and_predict(df, start_year, end_year)
        male_ratio, female_ratio = compute_gender_ratio(df)

        predictions = []
        for year in range(start_year, end_year + 1):
            total = totals_by_year[year]
            male = int(round(total * male_ratio))
            female = total - male
            predictions.append({
                "year": year,
                "total_births": total,
                "male_births": male,
                "female_births": female
            })

        print(json.dumps({
            "success": True,
            "model": MODEL_NAME,
            "predictions": predictions
        }))

    except Exception as exc:
        print(json.dumps({"success": False, "error": f"Prediction failed: {exc}"}))

if __name__ == "__main__":
    main()
