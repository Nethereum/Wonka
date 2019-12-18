namespace Wonka.BizRulesEngine
{
    public enum RULE_ERR_LVL
    {
        ERR_LVL_WARNING = 1,
        ERR_LVL_SEVERE,
        ERR_LVL_NONE
    }

    public enum RULE_SET_ERR_LVL
    {
        ERR_LVL_WARNING = 1,
        ERR_LVL_SEVERE,
        ERR_LVL_NONE
    }

    public enum COND_EVAL_MODE
    {
        MODE_AND = 1,
        MODE_OR,
        MODE_MAX
    }

    public enum RULE_OP
    {
        OP_NOT = 1,
        OP_AND,
        OP_OR,
        OP_NONE
    }

    public enum RULE_TYPE
    {
        RT_DOMAIN = 1,
        RT_POPULATED,
        RT_ARITH_LIMIT,
        RT_ASSIGNMENT,
        RT_DATE_LIMIT,
        RT_TRANSLATION,
        RT_COMPLEX,
        RT_PARSER,
        RT_ARITHMETIC,
        RT_CUSTOM_OP,
        RT_NONE
    }

    public enum ARITH_OP_TYPE
    {
        AOT_SUM = 1,
        AOT_DIFF,
        AOT_PROD,
        AOT_QUOT,
        AOT_NONE
    }

    public enum COMPLEX_RULE_ID
    {
        CRID_VALIDATE_ID = 1,
        CRID_MAX
    }

    public enum ERR_CD
    {
        CD_NOT_EXECUTED = -1,
        CD_SUCCESS,
        CD_FAILURE,
        CD_ARG_NUM_WRONG,
        CD_EXEC_ERROR,
        CD_MAX
    }

    public enum TARGET_RECORD
    {
        TRID_NEW_RECORD = 1,
        TRID_OLD_RECORD,
        TRID_NONE
    }

    public enum SOURCE_TYPE
    {
        SRC_TYPE_CONTRACT = 1,
        SRC_TYPE_API,
		SRC_TYPE_STORED_PROCEDURE,
        SRC_TYPE_NONE
    }

    public enum STD_OP_TYPE
    {
        STD_OP_BLOCK_NUM = 1,
        STD_OP_NONE
    }
}
