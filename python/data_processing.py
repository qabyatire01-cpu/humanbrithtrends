import pandas as pd
import numpy as np

def load_history_dataframe(history_records):
    
    df = pd.DataFrame(history_records)

    if df.empty:
        return df

    df["year"] = pd.to_numeric(df["year"], errors="coerce").astype("Int64")
    for col in ["total_births", "male_births", "female_births"]:
        df[col] = pd.to_numeric(df[col], errors="coerce")

    df = df.dropna(subset=["year", "total_births"])

    df = df[(df["total_births"] >= 0)]

    df = df.groupby("year", as_index=False).agg({
        "total_births": "sum",
        "male_births": "sum",
        "female_births": "sum"
    })

    df = df.sort_values("year").reset_index(drop=True)

    return df

def compute_gender_ratio(df):
    
    total_male = df["male_births"].sum()
    total_female = df["female_births"].sum()
    total = total_male + total_female

    if total == 0:
        return 0.512, 0.488

    return total_male / total, total_female / total
