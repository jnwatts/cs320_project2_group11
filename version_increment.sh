#!/bin/sh
function usage {
	echo "Usage $0 <options>"
    echo "-f <path> Version file to read from and update (required)"
	echo "-m Minor revision increase"
	echo "-M Major revision increase"
    echo "-b Build revision increase (only shown if this flag is set)"
    echo "-r <type> Include repository identifier (type is either svn or git)"
	echo "-h print this message"
	echo "-v print verbose debugging info"
}
OPTSTRING="f:mMbr:hv"

MAJOR_MOD=${MAJOR_MOD:-65536}
MINOR_MOD=${MINOR_MOD:-65536}
BUILD_MOD=${BUILD_MOD:-65536}

MAJOR=0
MINOR=0
BUILD=0
REPO=0
VERBOSE=0

DBG() {
    if [ "${VERBOSE}" -eq 1 ]; then
        echo "${*}"
    fi
}

while getopts ${OPTSTRING} c; do
	case ${c} in
	M) MAJOR=1;;
	m) MINOR=1;;
	b) BUILD=1;;
    r) REPO="${OPTARG}";;
    f) VERSION_PATH="${OPTARG}";;
	h) usage ; exit 0;;
    v) VERBOSE=1;;
	\?) echo "Invalid option: -${OPTARG}" >&2; usage; exit 1 ;;
	:) echo "Option -${OPTARG} requires an argument." >&2; usage; exit 1 ;;
	*) echo "Unimplemented option: -${OPTARG}" >&2; usage; exit 1;;
	esac
done
shift $((OPTIND-1))


INPUT_REV_STR="$(cat "${VERSION_PATH}" 2>/dev/null)"
declare -a INPUT_REV="(${INPUT_REV_STR//[.-]/ })"

MAJOR_REV="${INPUT_REV[0]:-0}"
MINOR_REV="${INPUT_REV[1]:-0}"
if [ "${BUILD}" -eq 1 ]; then
    BUILD_REV="${INPUT_REV[2]:-0}"
    REPO_REV="${INPUT_REV[3]}"
else
    REPO_REV="${INPUT_REV[2]}"
fi

DBG "Parsed ${INPUT_REV_STR} -> ${MAJOR_REV}.${MINOR_REV}.${BUILD_REV}-${REPO_REV}"

case "${REPO}" in
    0)
        ;;

    svn)
        REPO_REV="$(svnversion)"
        if [ "${REPO_REV}" == "exported" ]; then
            echo "-r svn requested, but this is not a subversion repository" >&2
            REPO_REV=""
        fi
        ;;

    git)
        REPO_REV="$(git rev-parse HEAD 2>/dev/null | sed 's/\(^[a-f0-9]\{7\}\).*/g\1/')"
        if [ "${REPO_REV}" == "g" ]; then
            echo "-r git requested, but this is not a git repository" >&2
            REPO_REV=""
        fi
        ;;

    *)
        echo "Unsupported value for -r: ${REPO}" >&2
        ;;
esac

if [ "${MAJOR}" -eq 1 ]
then
    DBG "Incrementing MAJOR"
	let "MAJOR_REV = (${MAJOR_REV} + 1) % ${MAJOR_MOD}"
    let "MINOR_REV = 0"
    let "BUILD_REV = 0"
elif [ "${MINOR}" -eq 1 ]
then
    DBG "Incrementing MINOR"
	let "MINOR_REV = (${MINOR_REV} + 1) % ${MINOR_MOD}"
    let "BUILD_REV = 0"
else
    DBG "Incrementing BUILD"
    let "BUILD_REV = (${BUILD_REV} + 1) % ${BUILD_MOD}"
fi

OUTPUT_REV="${MAJOR_REV}.${MINOR_REV}"
if [ "${BUILD}" -eq 1 ]; then
    OUTPUT_REV="${OUTPUT_REV}.${BUILD_REV}"
fi
if [ ! -z "${REPO_REV}" ]; then
    OUTPUT_REV="${OUTPUT_REV}-${REPO_REV}"
fi
echo "${OUTPUT_REV}"
echo "${OUTPUT_REV}" > "${VERSION_PATH}"
